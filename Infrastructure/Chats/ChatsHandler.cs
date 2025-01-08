using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Domain;

namespace Infrastructure.Chats;

public class ChatsHandler : IAsyncDisposable
{
    private ConcurrentDictionary<(int toClient, int sender), ConcurrentQueue<(string message, string time)>> _chats =
        new();

    private string _wssServerLink;
    private ChatConnection _chatConnection;
    private int _myId;
    public List<int> chatsToUpdate;
    private Action HandleChatsToUpdate;

    public ChatsHandler(int myId, Action handleChatsToUpdate, string serverLink = "ws://localhost:8080/ws/")
    {
        _chats = new ConcurrentDictionary<(int toClient, int sender), ConcurrentQueue<(string message, string time)>>();
        _myId = myId;
        _wssServerLink = serverLink;
        chatsToUpdate = new List<int>();
        _chatConnection = new ChatConnection(_wssServerLink, HandleNewMessage);
        HandleChatsToUpdate = handleChatsToUpdate;
    }

    public async Task StartSetUp()
    {
        await _chatConnection.OpenConnectionAsync();
        if (_chatConnection.IsOpen)
            await SendIdToServer();
        else
            Console.WriteLine("Не удалось установить начальное соединение с сервером");
    }

    public void AddChat(int userId) =>
        _chats.TryAdd((userId, _myId), new ConcurrentQueue<(string message, string time)>());

    public void DelChat(int userId) => _chats.TryRemove((userId, _myId), out _);

    public async Task UpdateAsync() => _ = _chatConnection.StartReceivingMessagesAsync();

    private void HandleNewMessage(string message)
    {
        Console.WriteLine($"Получено сообщение в HandleNewMessage {_myId}: {message} ");
        var chatMessage = ChatMessageFormatter.ParseClientMessage(message);

        if (chatMessage is not null)
        {
            var messageInfo = (chatMessage.MessageContext, chatMessage.MessageTime);
            var userMessages = _chats.GetOrAdd((chatMessage.RecieverId, chatMessage.SenderId),
                new ConcurrentQueue<(string message, string time)>());
            userMessages.Enqueue(messageInfo);
            SubscribeNewMessages(chatMessage.SenderId);
        }

        HandleChatsToUpdate();
    }

    public async Task<bool> SendMessage(int userId, string message)
    {
        var currTime = DateTimeOffset.UtcNow.ToString("o");
        AddChat(userId);
        if (await SendMessageToServer(userId, message, currTime))
        {
            _chats[(userId, _myId)].Enqueue((message, currTime));
            return true;
        }

        return false;
    }

    private async Task<bool> SendMessageToServer(int userId, string message, string currTime)
    {
        var formattedMessage = ChatMessageFormatter.CreateMessage(senderId: _myId, message: message, recieverId: userId,
            addInfo: "NONE", sendTime: currTime);
        return await _chatConnection.SendMessageAsync(formattedMessage);
    }

    private async Task SendIdToServer()
    {
        var message = $"[SRVID][{_myId}]";
        await _chatConnection.SendMessageAsync(message);
    }


    private void SubscribeNewMessages(int userId) => chatsToUpdate.Add(userId);

    public List<(string, string)> GetMessages(int userId)
    {
        var result = new List<(string message, string time)>();
        if (_chats.TryGetValue((userId, _myId), out var userMessagesFrom) && userMessagesFrom != null)
            result.AddRange(userMessagesFrom.ToList());
        if (_chats.TryGetValue((_myId, userId), out var userMessagesTo) && userMessagesTo != null)
            result.AddRange(userMessagesTo.ToList());

        return result;
    }

    public Dictionary<int, List<(string message, string time)>> GetAllChats()
    {
        var allChats = new Dictionary<int, List<(string message, string time)>>();

        foreach (var key in _chats.Keys)
        {
            if (key.sender == _myId || key.toClient == _myId)
            {
                int otherClientId = key.sender == _myId ? key.toClient : key.sender;

                if (!allChats.ContainsKey(otherClientId))
                    allChats[otherClientId] = new List<(string message, string time)>();

                if (_chats.TryGetValue((otherClientId, _myId), out var messagesFromOther) && messagesFromOther != null)
                    allChats[otherClientId].AddRange(messagesFromOther.ToList());

                if (_chats.TryGetValue((_myId, otherClientId), out var messagesToOther) && messagesToOther != null)
                    allChats[otherClientId].AddRange(messagesToOther.ToList());
            }
        }

        return allChats;
    }

    public async ValueTask DisposeAsync()
    {
        _chats.Clear();
        await _chatConnection.DisposeAsync();
    }
}
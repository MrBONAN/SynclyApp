using System.Collections.Concurrent;

namespace Infrastructure.Chats;

public class ChatsHandler : IAsyncDisposable
{
    private readonly ConcurrentDictionary<ChatPair, ConcurrentQueue<(string message, string time)>> _chats = new();
    private readonly ConcurrentDictionary<(int userId, ChatPair chatPair), int> _readMessageCounts = new();

    private string _wssServerLink;
    private ChatConnection _chatConnection;
    private readonly int _myId;
    public List<int> chatsToUpdate;
    private readonly Action HandleChatsToUpdate;

    public ChatsHandler(int myId, Action handleChatsToUpdate, string serverLink = "ws://localhost:8080/ws/")
    {
        _myId = myId;
        _wssServerLink = serverLink;
        HandleChatsToUpdate = handleChatsToUpdate;
        chatsToUpdate = new List<int>();
        _chatConnection = new ChatConnection(_wssServerLink, HandleNewMessage);
    }

    public async Task StartSetUp()
    {
        await _chatConnection.OpenConnectionAsync();
        if (_chatConnection.IsOpen)
            await SendIdToServer();
        else
            Console.WriteLine("Не удалось установить начальное соединение с сервером");
    }

    public void AddChat(int userId)
    {
        var chatPair = new ChatPair(_myId, userId);
        _chats.TryAdd(chatPair, new ConcurrentQueue<(string message, string time)>());
        _readMessageCounts.TryAdd((_myId, chatPair), 0);
        _readMessageCounts.TryAdd((userId, chatPair), 0);
    }

    public void DelChat(int userId)
    {
        var chatPair = new ChatPair(_myId, userId);
        _chats.TryRemove(chatPair, out _);
        _readMessageCounts.TryRemove((_myId, chatPair), out _);
        _readMessageCounts.TryRemove((userId, chatPair), out _);
    }

    public async Task UpdateAsync()
    {
        await _chatConnection.StartReceivingMessagesAsync();
    }

    private void HandleNewMessage(string message)
    {
        var chatMessage = ChatMessageFormatter.ParseClientMessage(message);

        if (chatMessage is not null)
        {
            var chatPair = new ChatPair(chatMessage.SenderId, chatMessage.RecieverId);
            var messageInfo = (chatMessage.MessageContext, chatMessage.MessageTime);

            var userMessages = _chats.GetOrAdd(chatPair, new ConcurrentQueue<(string message, string time)>());
            userMessages.Enqueue(messageInfo);

            _readMessageCounts.TryAdd((chatMessage.SenderId, chatPair), 0);
            _readMessageCounts.TryAdd((chatMessage.RecieverId, chatPair), 0);

            // Уведомляем только получателя о новом сообщении
            if (chatMessage.RecieverId == _myId)
            {
                chatsToUpdate.Add(chatMessage.SenderId);
            }
        }
        else
            Console.WriteLine("Ошибка: не удалось разобрать входящее сообщение");

        HandleChatsToUpdate();
    }

    public async Task<bool> SendMessage(int userId, string message)
    {
        var currTime = DateTimeOffset.UtcNow.ToString("o");
        AddChat(userId);
        if (await SendMessageToServer(userId, message, currTime))
        {
            var chatPair = new ChatPair(_myId, userId);
            _chats[chatPair].Enqueue((message, currTime));
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
        var chatPair = new ChatPair(_myId, userId);
        var result = new List<(string message, string time)>();

        if (_chats.TryGetValue(chatPair, out var messages) && messages != null)
        {
            result.AddRange(messages.ToList());
            _readMessageCounts[(_myId, chatPair)] = messages.Count;
        }

        return result.OrderBy(x => DateTimeOffset.Parse(x.time)).ToList();
    }

    public List<(string, string)> GetNewMessages(int userId)
    {
        var chatPair = new ChatPair(_myId, userId);
        var result = new List<(string message, string time)>();

        if (_chats.TryGetValue(chatPair, out var messages) && messages != null)
        {
            var readCount = _readMessageCounts.GetOrAdd((_myId, chatPair), 0);
            var newMessages = messages.ToList().Skip(readCount);
            result.AddRange(newMessages);
            _readMessageCounts[(_myId, chatPair)] = messages.Count;
        }

        return result.OrderBy(x => DateTimeOffset.Parse(x.time)).ToList();
    }

    public Dictionary<int, List<(string message, string time)>> GetAllChats()
    {
        var result = new Dictionary<int, List<(string message, string time)>>();
        foreach (var chatEntry in _chats)
        {
            var chatPair = chatEntry.Key;
            if (chatPair.Contains(_myId))
            {
                var otherUserId = chatPair.Id1 == _myId ? chatPair.Id2 : chatPair.Id1;
                if (!result.ContainsKey(otherUserId))
                    result[otherUserId] = new List<(string message, string time)>();
                result[otherUserId].AddRange(chatEntry.Value);
            }
        }
        return result;
    }

    public async Task RestoreMessage(int userId, string message, string time)
    {
        var chatPair = new ChatPair(_myId, userId);
        var userMessages = _chats.GetOrAdd(chatPair, new ConcurrentQueue<(string message, string time)>());
        userMessages.Enqueue((message, time));
    }

    public async ValueTask DisposeAsync()
    {
        if (_chatConnection != null)
        {
            await _chatConnection.DisposeAsync();
        }
    }
}
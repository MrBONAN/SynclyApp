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
    
    public bool isOpen() => _chatConnection.IsOpen;

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
        if (chatMessage != null)
        {
            var chatPair = new ChatPair(_myId, chatMessage.SenderId);
            var userMessages = _chats.GetOrAdd(chatPair, new ConcurrentQueue<(string message, string time)>());

            // Инициализируем счетчики для обоих участников чата
            _readMessageCounts.TryAdd((chatMessage.SenderId, chatPair), 0);
            _readMessageCounts.TryAdd((chatMessage.RecieverId, chatPair), 0);

            switch (chatMessage.AdditionalInfo)
            {
                case "EDIT":
                    // Находим сообщение для редактирования и обновляем его
                    var editList = userMessages.ToList();
                    var editIndex = editList.FindIndex(m => m.time == chatMessage.MessageTime);
                    if (editIndex != -1)
                    {
                        editList[editIndex] = (chatMessage.MessageContext, chatMessage.MessageTime);
                        // Очищаем и заново заполняем очередь
                        while (userMessages.TryDequeue(out _)) { }
                        foreach (var msg in editList)
                        {
                            userMessages.Enqueue(msg);
                        }
                    }
                    break;

                case "DELETE":
                    // Находим сообщение для удаления
                    var deleteList = userMessages.ToList();
                    var deleteIndex = deleteList.FindIndex(m => m.time == chatMessage.MessageTime);
                    if (deleteIndex != -1)
                    {
                        deleteList.RemoveAt(deleteIndex);
                        // Очищаем и заново заполняем очередь
                        while (userMessages.TryDequeue(out _)) { }
                        foreach (var msg in deleteList)
                        {
                            userMessages.Enqueue(msg);
                        }
                    }
                    break;

                default: // "NONE" - обычное сообщение
                    userMessages.Enqueue((chatMessage.MessageContext, chatMessage.MessageTime));
                    break;
            }

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
            // Берем только обычные сообщения (не EDIT/DELETE) и сортируем их по времени
            result.AddRange(messages.Where(m => !m.time.StartsWith("EDIT") && !m.time.StartsWith("DELETE")).ToList());
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

    // Методы для работы с непрочитанными сообщениями
    public int GetUnreadCount(int userId)
    {
        var chatPair = new ChatPair(_myId, userId);
        if (_chats.TryGetValue(chatPair, out var messages) && messages != null)
        {
            var readCount = _readMessageCounts.GetOrAdd((_myId, chatPair), 0);
            return messages.Count - readCount;
        }
        return 0;
    }

    public void MarkAsRead(int userId)
    {
        var chatPair = new ChatPair(_myId, userId);
        if (_chats.TryGetValue(chatPair, out var messages) && messages != null)
        {
            _readMessageCounts[(_myId, chatPair)] = messages.Count;
        }
    }

    // Методы редактирования и удаления сообщений
    public async Task<bool> EditMessage(int userId, string messageTime, string newText)
    {
        var chatPair = new ChatPair(_myId, userId);
        if (!_chats.TryGetValue(chatPair, out var messages) || messages == null)
            return false;

        var messagesList = messages.ToList();
        var messageIndex = messagesList.FindIndex(m => m.time == messageTime);
        if (messageIndex == -1)
            return false;

        messagesList[messageIndex] = (newText, messageTime);
        
        // Очищаем и заново заполняем очередь
        while (messages.TryDequeue(out _)) { }
        foreach (var msg in messagesList)
        {
            messages.Enqueue(msg);
        }

        // Отправляем уведомление об изменении
        var editMessage = ChatMessageFormatter.CreateMessage(_myId, newText, userId, messageTime, "EDIT");
        return await _chatConnection.SendMessageAsync(editMessage);
    }

    public async Task<bool> DeleteMessage(int userId, string messageTime)
    {
        var chatPair = new ChatPair(_myId, userId);
        if (!_chats.TryGetValue(chatPair, out var messages) || messages == null)
            return false;

        var messagesList = messages.ToList();
        var messageIndex = messagesList.FindIndex(m => m.time == messageTime);
        if (messageIndex == -1)
            return false;

        messagesList.RemoveAt(messageIndex);
        
        // Очищаем и заново заполняем очередь
        while (messages.TryDequeue(out _)) { }
        foreach (var msg in messagesList)
        {
            messages.Enqueue(msg);
        }

        // Отправляем уведомление об удалении
        var deleteMessage = ChatMessageFormatter.CreateMessage(_myId, "", userId, messageTime, "DELETE");
        return await _chatConnection.SendMessageAsync(deleteMessage);
    }

    // Методы управления соединением
    public async Task ReconnectAsync()
    {
        await _chatConnection.DisposeAsync();
        _chatConnection = new ChatConnection(_wssServerLink, HandleNewMessage);
        await StartSetUp();
    }

    public async ValueTask DisposeAsync()
    {
        if (_chatConnection != null)
        {
            await _chatConnection.DisposeAsync();
        }
    }
}
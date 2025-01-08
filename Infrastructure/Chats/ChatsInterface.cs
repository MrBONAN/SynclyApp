namespace Infrastructure.Chats;

public class Chats : IAsyncDisposable
{
    private readonly ChatsHandler _chatsHandler;
    private bool _isRunning;
    public event EventHandler<List<int>> NewMessagesReceived;
    private readonly List<int> _newMessagesUsers = new();

    public Chats(int myId, string serverLink = "ws://localhost:8080/ws/")
    {
        _chatsHandler = new ChatsHandler(myId, ProcessChatsToUpdate, serverLink);
    }

    public async Task RunAsync()
    {
        if (_isRunning)
            throw new InvalidOperationException("Служба Chats уже запущена.");

        await _chatsHandler.StartSetUp();
        _isRunning = true;

        _ = Task.Run(async () =>
        {
            try
            {
                await _chatsHandler.UpdateAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в процессе обновления чатов: {ex.Message}");
            }
            finally
            {
                _isRunning = false;
            }
        });
    }


    private void ProcessChatsToUpdate()
    {
        lock (_newMessagesUsers)
        {
            if (_chatsHandler.chatsToUpdate != null && _chatsHandler.chatsToUpdate.Count > 0)
            {
                _newMessagesUsers.AddRange(_chatsHandler.chatsToUpdate);
                _chatsHandler.chatsToUpdate.Clear();

                var newMessages = _newMessagesUsers.Distinct().ToList();
                _newMessagesUsers.Clear();
                OnNewMessagesReceived(newMessages);
            }
        }
    }

    protected virtual void OnNewMessagesReceived(List<int> newMessages) => NewMessagesReceived?.Invoke(this, newMessages);
    

    private async Task StopAsync()
    {
        _isRunning = false;
        await Task.CompletedTask;
    }

    public async Task<bool> SendMessageAsync(int userId, string message) => await _chatsHandler.SendMessage(userId: userId, message: message);

    public List<(string, string)> GetMessages(int userId) => _chatsHandler.GetMessages(userId);
    
    public List<(string, string)> GetNewMessages(int userId) => _chatsHandler.GetNewMessages(userId);
    
    public Dictionary<int, List<(string message, string time)>> GetAllChats() => _chatsHandler.GetAllChats();

    public async Task RestoreMessage(int userId, string message, string time) => await _chatsHandler.RestoreMessage(userId, message, time);

    public void AddChat(int userId) => _chatsHandler.AddChat(userId);
    public void RemoveChat(int userId) => _chatsHandler.DelChat(userId);
    
    public bool IsConnected => _chatsHandler.isOpen();
    public bool IsRunning => _isRunning;
    
    public int GetUnreadCount(int userId) => _chatsHandler.GetUnreadCount(userId);
    public void MarkAsRead(int userId) => _chatsHandler.MarkAsRead(userId);
    public async Task<bool> EditMessage(int userId, string oldMessageTime, string newText) => 
        await _chatsHandler.EditMessage(userId, oldMessageTime, newText);
    public async Task<bool> DeleteMessage(int userId, string messageTime) => 
        await _chatsHandler.DeleteMessage(userId, messageTime);
    
    public async Task ReconnectAsync() => await _chatsHandler.ReconnectAsync();
    public async Task DisconnectAsync() => await StopAsync();

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
        await _chatsHandler.DisposeAsync();
    }

    public async Task StopChat() => await DisposeAsync();
}
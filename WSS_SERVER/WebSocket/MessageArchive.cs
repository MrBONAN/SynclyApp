using Domain;

namespace WebSocketServer;

public class MessageArchive : IMessageArchive
{
    private readonly Dictionary<int, HashSet<(string message, DateTime timestamp)>> _messagesToSend = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly IClientManager _clientManager;
    private readonly TimeSpan _messageRetentionPeriod = TimeSpan.FromMinutes(5);
    private static CancellationTokenSource _cts = new();

    public MessageArchive(IClientManager clientManager)
    {
        _clientManager = clientManager;
    }

    public async Task StartAsync()
    {
        _ = Task.Run(() => StartCleanupTimer(_cts.Token));
        _ = Task.Run(() => StartMessageResendTimer(_cts.Token));
    }

    public async Task ArchiveMessage(ChatMessage message)
    {
        await _semaphore.WaitAsync();
        try
        {
            if (!_messagesToSend.ContainsKey(message.RecieverId))
                _messagesToSend[message.RecieverId] = new HashSet<(string, DateTime)>();
            _messagesToSend[message.RecieverId].Add((ChatMessageFormatter.CreateMessage(message), DateTime.UtcNow));
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task SendArchivedMessages(int clientId)
    {
        await _semaphore.WaitAsync();
        try
        {
            if (_messagesToSend.ContainsKey(clientId))
            {
                var messages = _messagesToSend[clientId].ToList();
                var successfullySent = new List<(string, DateTime)>();

                foreach (var (messageToSend, timestamp) in messages)
                {
                    var messageParsed = ChatMessageFormatter.ParseClientMessage(messageToSend);
                    if (messageParsed != null)
                        if (await _clientManager.SendMessageToClientAsync(messageParsed.SenderId, messageToSend,
                                messageParsed.RecieverId))
                            successfullySent.Add((messageToSend, timestamp));
                }

                foreach (var sent in successfullySent)
                    _messagesToSend[clientId].Remove(sent);

                if (_messagesToSend[clientId].Count == 0)
                    _messagesToSend.Remove(clientId);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SendArchivedMessages] Ошибка при отправке архивных сообщений: {ex.Message}");
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task StartMessageResendTimer(CancellationToken token)
    {
        Console.WriteLine("[StartMessageResendTimer] Запущен таймер повторной отправки сообщений");
        while (true)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    Console.WriteLine("Остановка таймера очистки.");
                    token.ThrowIfCancellationRequested();
                }

                await Task.Delay(TimeSpan.FromSeconds(10)); // Пробуем каждые 10 секунд

                var clientIds = _messagesToSend.Keys.ToList();
                foreach (var clientId in clientIds)
                {
                    try
                    {
                        if (_clientManager.HasClient(clientId))
                        {
                            Console.WriteLine(
                                $"[StartMessageResendTimer] Клиент {clientId} онлайн, пробуем отправить сообщения");
                            await SendArchivedMessages(clientId);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"[StartMessageResendTimer] Ошибка при повторной отправке сообщений клиенту {clientId}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[StartMessageResendTimer] Ошибка в таймере повторной отправки: {ex.Message}");
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }
    }

    private async Task StartCleanupTimer(CancellationToken token)
    {
        Console.WriteLine("[StartCleanupTimer] Запущен таймер очистки старых сообщений");
        while (true)
        {
            if (token.IsCancellationRequested)
            {
                Console.WriteLine("Остановка таймера очистки.");
                token.ThrowIfCancellationRequested();
            }

            try
            {
                await Task.Delay(TimeSpan.FromMinutes(1));
                await _semaphore.WaitAsync();

                try
                {
                    var now = DateTime.UtcNow;
                    var clientsToRemove = new List<int>();

                    foreach (var (clientId, messages) in _messagesToSend)
                    {
                        var oldMessages = messages.Where(m => now - m.timestamp > _messageRetentionPeriod).ToList();
                        foreach (var oldMessage in oldMessages)
                        {
                            messages.Remove(oldMessage);
                            Console.WriteLine($"[StartCleanupTimer] Удалено старое сообщение для клиента {clientId}");
                        }

                        if (messages.Count == 0)
                            clientsToRemove.Add(clientId);
                    }

                    foreach (var clientId in clientsToRemove)
                    {
                        _messagesToSend.Remove(clientId);
                        Console.WriteLine($"[StartCleanupTimer] Удалены все сообщения для клиента {clientId}");
                    }
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[StartCleanupTimer] Ошибка при очистке старых сообщений: {ex.Message}");
            }
        }
    }

    public async Task StopAsync() => _cts.Cancel();
}
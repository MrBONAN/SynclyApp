using Domain;

namespace WebSocketServer;

public class MessageArchive : IMessageArchive
{
    private readonly Dictionary<int, HashSet<string>> _messagesToSend = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly IClientManager _clientManager;
    
    public MessageArchive(IClientManager clientManager)
    {
        _clientManager = clientManager;
    }

    public async Task ArchiveMessage(ChatMessage message)
    {
        Console.WriteLine(
            $"HandleConnectionAsync]Archiving new messsage {message.SenderId} -> {message.RecieverId}");
        await _semaphore.WaitAsync();
        try
        {
            if (!_messagesToSend.ContainsKey(message.RecieverId))
                _messagesToSend[message.RecieverId] = new HashSet<string>();
            _messagesToSend[message.RecieverId].Add(ChatMessageFormatter.CreateMessage(message));
            Console.WriteLine($"Message archived {ChatMessageFormatter.CreateMessage(message)}");
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
                var messagesToSend = _messagesToSend[clientId].ToList();
                var successfullySentMessages = new List<string>();

                foreach (var messageToSend in messagesToSend)
                {
                    var messageParsed = ChatMessageFormatter.ParseClientMessage(messageToSend);

                    var sent = await _clientManager.SendMessageToClientAsync(messageParsed.SenderId, messageToSend,
                        messageParsed.RecieverId);
                    if (sent)
                        successfullySentMessages.Add(messageToSend);
                }

                foreach (var sentMessage in successfullySentMessages)
                    _messagesToSend[clientId].Remove(sentMessage);

                if (_messagesToSend[clientId].Count == 0)
                    _messagesToSend.Remove(clientId);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при отправке архивных сообщений: {ex.Message}");
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task StartMessageResendTimer()
    {
        while (true)
        {
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(1));

                var clientIds = _messagesToSend.Keys.ToList();
                foreach (var clientId in clientIds)
                {
                    try
                    {
                        if (_clientManager.HasClient(clientId))
                            await SendArchivedMessages(clientId);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при повторной отправке сообщений клиенту {clientId}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в таймере повторной отправки: {ex.Message}");
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }
    }
}

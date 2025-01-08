using System.Net.WebSockets;
using System.Text;
using Domain;

namespace WebSocketServer;

public class MessageHandler : IMessageHandler
{
    private readonly IClientManager _clientManager;
    private readonly IMessageArchive _messageArchive;
    
    public MessageHandler(IClientManager clientManager, IMessageArchive messageArchive)
    {
        _clientManager = clientManager;
        _messageArchive = messageArchive;
    }

    public async Task HandleConnectionAsync(WebSocket webSocket)
    {
        byte[] buffer = new byte[1024];
        int clientId = -1;
        try
        {
            while (webSocket.State == WebSocketState.Open)
            {
                string receivedMessage = await ReceiveMessageAsync(webSocket, buffer);
                if (receivedMessage is null) 
                {
                    Console.WriteLine($"[HandleConnectionAsync] Получено пустое сообщение от клиента {clientId}");
                    break;
                }

                Console.WriteLine($"Получено сообщение: {receivedMessage}");

                if (receivedMessage.StartsWith("[SRVID]"))
                {
                    clientId = await RegisterClient(receivedMessage, webSocket);
                    if (clientId != -1)
                        await _messageArchive.SendArchivedMessages(clientId);
                }
                else if (clientId != -1)
                    await HandleClientMessageAsync(receivedMessage, clientId);
                else
                    Console.WriteLine("Клиент не указал ID.");
            }
        }
        catch (WebSocketException wsEx)
        {
            Console.WriteLine($"[HandleConnectionAsync] WebSocket ошибка для клиента {clientId}: {wsEx.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[HandleConnectionAsync] Неожиданная ошибка для клиента {clientId}: {ex.Message}");
        }
        finally
        {
            if (clientId != -1)
                await _clientManager.CleanupClientAsync(clientId, webSocket);
        }
    }

    public async Task<string> ReceiveMessageAsync(WebSocket webSocket, byte[] buffer)
    {
        if (webSocket.State != WebSocketState.Open) return null;
        using (var memoryStream = new MemoryStream())
        {
            try
            {
                WebSocketReceiveResult result;
                do
                {
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        //Console.WriteLine("[ReceiveMessageAsync]Клиент запросил закрытие соединения.");
                        await _clientManager.CloseConnectionAsync(webSocket);
                        return null;
                    }

                    memoryStream.Write(buffer, 0, result.Count);
                } while (!result.EndOfMessage);

                var message = Encoding.UTF8.GetString(memoryStream.ToArray());
                return message;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ReceiveMessageAsync]Ошибка при получении сообщения: {ex.InnerException}");
                return null;
            }
        }
    }

    public async Task<int> RegisterClient(string message, WebSocket webSocket)
    {
        int clientId = int.Parse(message.Substring(8).Trim('\"', ' ', '[', ']', '\"'));
        await _clientManager.AddClient(clientId, webSocket);
        Console.WriteLine($"[RegisterClient]Клиент с ID '{clientId}' добавлен.");
        return clientId;
    }

    public async Task HandleClientMessageAsync(string receivedMessage, int senderId)
    {
        var message = ChatMessageFormatter.ParseClientMessage(receivedMessage);

        if (message is null) return;

        var responseMessage = ChatMessageFormatter.CreateMessage(message);
        
        var messageSent = await _clientManager.SendMessageToClientAsync(message.SenderId, responseMessage, message.RecieverId);
        
        if (!messageSent)
        {
            Console.WriteLine($"[HandleClientMessageAsync] Получатель {message.RecieverId} не в сети, сохраняем сообщение");
            await _messageArchive.ArchiveMessage(message);
        }
        Console.WriteLine("Message was sent");
    }

    private async Task CloseConnectionAsync(WebSocket webSocket)
    {
        if (webSocket?.State == WebSocketState.Open)
        {
            try
            {
                await webSocket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Закрытие соединения",
                    CancellationToken.None
                );
            }
            finally
            {
                webSocket.Dispose();
            }
        }
    }
}

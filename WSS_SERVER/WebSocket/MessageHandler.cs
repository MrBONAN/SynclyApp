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
                if (receivedMessage is null) break;

                Console.WriteLine($"Получено сообщение: {receivedMessage}");

                if (receivedMessage.StartsWith("[SRVID]"))
                    clientId = await RegisterClient(receivedMessage, webSocket);
                else if (clientId != -1)
                    await HandleClientMessageAsync(receivedMessage, clientId);
                else
                    Console.WriteLine("Клиент не указал ID.");
            }
        }
        catch (Exception ex)
        {
            //Console.WriteLine($"[HandleConnectionAsync]Ошибка в WebSocket-соединении: {ex.Message}");
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
        await _messageArchive.SendArchivedMessages(clientId);
        return clientId;
    }

    public async Task HandleClientMessageAsync(string receivedMessage, int senderId)
    {
        var message = ChatMessageFormatter.ParseClientMessage(receivedMessage);

        if (message is null) return;

        var responseMessage = ChatMessageFormatter.CreateMessage(message);
        var messageSent = false;
        var count = 0;
        while (!messageSent && count < 3)
        {
            messageSent = await _clientManager.SendMessageToClientAsync(message.SenderId, responseMessage, message.RecieverId);
            if (!messageSent)
            {
                count += 1;
                Console.WriteLine($"Не удалось отправить, осталось попыток: {3 - count}");
                if (count < 3) await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        if (!messageSent)
            await _messageArchive.ArchiveMessage(message);
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

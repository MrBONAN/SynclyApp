using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace WebSocketServer;

public class ClientManager : IClientManager
{
    private readonly ConcurrentDictionary<int, WebSocket> _clients = new();

    public async Task<bool> AddClient(int clientId, WebSocket webSocket)
    {
        if (_clients.TryGetValue(clientId, out var existingSocket))
            await CleanupClientAsync(clientId, existingSocket);

        return _clients.TryAdd(clientId, webSocket);
    }

    public async Task<bool> RemoveClient(int clientId)
    {
        if (_clients.TryGetValue(clientId, out var socket))
        {
            await CleanupClientAsync(clientId, socket);
            return true;
        }
        return false;
    }

    public async Task<bool> SendMessageToClientAsync(int senderId, string message, int receiverId)
    {
        Console.WriteLine("WOW:" + string.Join(", ", _clients.Keys));
        if (_clients.TryGetValue(receiverId, out var clientSocket) && clientSocket.State == WebSocketState.Open)
        {
            var responseBuffer = Encoding.UTF8.GetBytes(message);
            try
            {
                await clientSocket.SendAsync(new ArraySegment<byte>(responseBuffer), WebSocketMessageType.Text, true,
                    CancellationToken.None);
                Console.WriteLine($"[SendMessageToClient] Ответ отправлен клиенту {receiverId} от {senderId}.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"[SendMessageToClient] Ошибка при отправке сообщения клиенту {receiverId}: {ex.Message}");
                await CleanupClientAsync(receiverId, clientSocket);
                return false;
            }
        }

        Console.WriteLine($"[SendMessageToClientAsync] Клиент с ID {receiverId} не найден или его соединение закрыто.");
        return false;
    }

    public bool HasClient(int clientId) => _clients.ContainsKey(clientId);

    public async Task CleanupClientAsync(int clientId, WebSocket webSocket)
    {
        if (webSocket == null || webSocket.State == WebSocketState.Closed) return;

        try
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Соединение закрыто",
                CancellationToken.None);
            Console.WriteLine($"[CleanupClientAsync] Соединение с клиентом {clientId} закрыто.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"[CleanupClientAsync] Ошибка при закрытии соединения для клиента {clientId}: {ex.Message}");
        }
        finally
        {
            webSocket?.Dispose();

            if (_clients.TryRemove(clientId, out _))
            {
                Console.WriteLine($"[CleanupClientAsync] Клиент со ID '{clientId}' удален.");
            }
        }
    }

    public async Task CloseConnectionAsync(WebSocket webSocket)
    {
        //Console.WriteLine("[CloseConnectionAsync]Клиент запросил закрытие соединения.");
        if (webSocket != null && webSocket.State != WebSocketState.Closed)
        {
            try
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Закрытие соединения",
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"[CloseConnectionAsync]Ошибка при закрытии сокета: {ex.Message}");
            }
        }

        if (webSocket != null) webSocket.Dispose();
    }

    public async Task StartConnectionHealthCheck()
    {
        while (true)
        {
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(5)); // Проверка каждые 5 минут

                var disconnectedClients = _clients
                    .Where(c => c.Value.State != WebSocketState.Open)
                    .Select(c => c.Key)
                    .ToList();

                foreach (var clientId in disconnectedClients)
                {
                    if (_clients.TryGetValue(clientId, out var socket))
                    {
                        await CleanupClientAsync(clientId, socket);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при проверке состояния подключений: {ex.Message}");
            }
        }
    }
}

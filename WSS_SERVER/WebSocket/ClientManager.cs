using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace WebSocketServer;

public class ClientManager : IClientManager
{
    private readonly ConcurrentDictionary<int, WebSocket> _clients = new();
    private Task _connectionHeathCheckTask { get; set; }
    private CancellationTokenSource _cts = new();

    public async Task StartAsync() =>
        _connectionHeathCheckTask = Task.Run(() => StartConnectionHealthCheck(_cts.Token));

    public async Task<bool> AddClient(int clientId, WebSocket webSocket)
    {
        if (_clients.TryGetValue(clientId, out var existingSocket))
        {
            Console.WriteLine($"[AddClient] Клиент {clientId} уже существует, очищаем старое соединение");
            await CleanupClientAsync(clientId, existingSocket);
        }

        var added = _clients.TryAdd(clientId, webSocket);
        if (added)
        {
            Console.WriteLine($"[AddClient] Клиент {clientId} успешно добавлен");
            Console.WriteLine($"[AddClient] Активные клиенты: {string.Join(", ", _clients.Keys)}");
        }

        return added;
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
        Console.WriteLine($"[SendMessageToClientAsync] Активные клиенты: {string.Join(", ", _clients.Keys)}");
        if (_clients.TryGetValue(receiverId, out var clientSocket) && clientSocket.State == WebSocketState.Open)
        {
            var responseBuffer = Encoding.UTF8.GetBytes(message);
            try
            {
                await clientSocket.SendAsync(new ArraySegment<byte>(responseBuffer), WebSocketMessageType.Text, true,
                    CancellationToken.None);
                Console.WriteLine(
                    $"[SendMessageToClientAsync] Сообщение отправлено клиенту {receiverId} от {senderId}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"[SendMessageToClientAsync] Ошибка при отправке сообщения клиенту {receiverId}: {ex.Message}");
                await CleanupClientAsync(receiverId, clientSocket);
                return false;
            }
        }

        Console.WriteLine($"[SendMessageToClientAsync] Клиент с ID {receiverId} не найден или его соединение закрыто");
        return false;
    }

    public bool HasClient(int clientId)
    {
        var hasClient = _clients.TryGetValue(clientId, out var socket) && socket.State == WebSocketState.Open;
        return hasClient;
    }

    public async Task CleanupClientAsync(int clientId, WebSocket webSocket)
    {
        if (webSocket == null || webSocket.State == WebSocketState.Closed)
        {
            Console.WriteLine($"[CleanupClientAsync] Соединение с клиентом {clientId} уже закрыто");
            return;
        }

        try
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Соединение закрыто",
                CancellationToken.None);
            Console.WriteLine($"[CleanupClientAsync] Соединение с клиентом {clientId} закрыто");
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
                Console.WriteLine($"[CleanupClientAsync] Клиент {clientId} удален");
                Console.WriteLine($"[CleanupClientAsync] Активные клиенты: {string.Join(", ", _clients.Keys)}");
            }
        }
    }

    public async Task CloseConnectionAsync(WebSocket webSocket)
    {
        Console.WriteLine("[CloseConnectionAsync] Клиент запросил закрытие соединения");
        if (webSocket != null && webSocket.State != WebSocketState.Closed)
        {
            try
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Закрытие соединения",
                    CancellationToken.None);
                Console.WriteLine("[CloseConnectionAsync] Соединение успешно закрыто");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CloseConnectionAsync] Ошибка при закрытии сокета: {ex.Message}");
            }
        }

        webSocket?.Dispose();
    }

    public async Task StartConnectionHealthCheck(CancellationToken token)
    {
        while (true)
        {
            if (token.IsCancellationRequested)
            {
                Console.WriteLine("Остановка таймера очистки.");
                token.ThrowIfCancellationRequested();
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(10));

                var disconnectedClients = _clients
                    .Where(c => c.Value.State != WebSocketState.Open)
                    .Select(c => c.Key)
                    .ToList();

                if (disconnectedClients.Any())
                {
                    Console.WriteLine(
                        $"[StartConnectionHealthCheck] Найдены отключенные клиенты: {string.Join(", ", disconnectedClients)}");
                    foreach (var clientId in disconnectedClients)
                        if (_clients.TryGetValue(clientId, out var socket))
                            await CleanupClientAsync(clientId, socket);
                }
                else
                    Console.WriteLine(
                        $"[StartConnectionHealthCheck] Все клиенты активны: {string.Join(", ", _clients.Keys)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"[StartConnectionHealthCheck] Ошибка при проверке состояния подключений: {ex.Message}");
            }
        }
    }

    public async Task StopAsync() => _cts.Cancel();
}
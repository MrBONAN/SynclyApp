using System.Net.WebSockets;
using System.Text;

public class ChatConnection : IAsyncDisposable
{
    private ClientWebSocket _webSocket;
    private Uri _serverUri;
    private int _connectionAttempts;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    public bool IsOpen { get; private set; }
    public event Action<string> MessageReceived;

    public ChatConnection(string serverUri, Action<string> onMessageReceived)
    {
        _serverUri = new Uri(serverUri);
        _webSocket = new ClientWebSocket();
        MessageReceived += onMessageReceived;
    }

    public async Task OpenConnectionAsync()
    {
        _connectionAttempts = 0;

        while (_connectionAttempts < 3)
        {
            try
            {
                Console.WriteLine(
                    $"[OpenConnectionAsync]Подключение к серверу: {_serverUri}, попытка {_connectionAttempts + 1}, До подключения");
                await _webSocket.ConnectAsync(_serverUri, CancellationToken.None);
                Console.WriteLine("[OpenConnectionAsync]Соединение установлено.");
                IsOpen = true;
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[openConnectionAsync]Ошибка при подключении: {ex.Message}, Повтор через 2 сек");
                await Task.Delay(2000);
                _connectionAttempts++;
            }
        }

        Console.WriteLine($"Не удалось установить соединение");
    }

    public async Task<bool> SendMessageAsync(string message)
    {
        if (!CheckConnection()) return false;
        try
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            var buffer = new ArraySegment<byte>(messageBytes);

            await _webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
            Console.WriteLine($"[SendMessageAsync]Сообщение отправлено: {message}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SendMessageAsync]Ошибка при отправке сообщения: {ex.Message}");
            if (_webSocket.State != WebSocketState.Open) IsOpen = false;
            return false;
        }
    }

    public async Task StartReceivingMessagesAsync()
    {
        byte[] buffer = new byte[1024];
        WebSocketReceiveResult result;

        try
        {
            while (_webSocket.State == WebSocketState.Open)
            {
                var arraySegment = new ArraySegment<byte>(buffer);
                result = await _webSocket.ReceiveAsync(arraySegment, _cancellationTokenSource.Token);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Console.WriteLine("Сервер закрыл соединение");
                    await DisposeAsync();
                    return;
                }

                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                MessageReceived?.Invoke(message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[StartReceivingMessagesAsync]Ошибка при получении сообщения: {ex.Message}");
        }
    }

    private bool CheckConnection() => _webSocket != null && _webSocket.State == WebSocketState.Open;

    public async Task CloseConnectionAsync(WebSocketCloseStatus closeStatus, string description)
    {
        try
        {
            if (CheckConnection())
            {
                Console.WriteLine($"Закрытие соединения, статус: {_webSocket.State}");
                await _webSocket.CloseAsync(closeStatus, description, CancellationToken.None);
                Console.WriteLine("Соединение закрыто.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при закрытии соединения: {ex.Message}");
        }

        IsOpen = false;
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            Console.WriteLine($"Закрытие соединения, статус: {_webSocket?.State}");
            await CloseConnectionAsync(WebSocketCloseStatus.NormalClosure, "Dispose");
            _webSocket?.Dispose();
            Console.WriteLine("Ресурсы освобождены.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при закрытии соединения: {ex.Message}");
        }
    }
}
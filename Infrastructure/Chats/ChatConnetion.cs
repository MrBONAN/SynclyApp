using System.Net.WebSockets;
using System.Text;
using System.Threading;

public class ChatConnection : IAsyncDisposable
{
    private ClientWebSocket _webSocket;
    private readonly Uri _serverUri;
    private int _connectionAttempts;
    private CancellationTokenSource _cancellationTokenSource;
    public bool IsOpen { get; private set; }
    public event Action<string>? MessageReceived;

    private readonly int _maxReconnectAttempts = 3;
    private readonly TimeSpan _reconnectDelay = TimeSpan.FromMilliseconds(500);
    private bool _isReconnecting;
    private bool _isDisposed;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private Task? _receiveTask;

    public ChatConnection(string serverUri, Action<string> onMessageReceived)
    {
        _serverUri = new Uri(serverUri);
        MessageReceived = onMessageReceived;
        _webSocket = new ClientWebSocket();
        _cancellationTokenSource = new CancellationTokenSource();
    }
    

    public async Task OpenConnectionAsync()
    {
        _webSocket = new ClientWebSocket();
        _cancellationTokenSource = new CancellationTokenSource();

        while (!_cancellationTokenSource.Token.IsCancellationRequested && !IsOpen)
        {
            try
            {
                if (_webSocket.State != WebSocketState.None && _webSocket.State != WebSocketState.Closed)
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing before retry",
                        CancellationToken.None);

                _webSocket = new ClientWebSocket();
                await _webSocket.ConnectAsync(_serverUri, _cancellationTokenSource.Token);
                IsOpen = true;

                _receiveTask = StartReceivingMessagesAsync();

                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка подключения: {ex.Message}");

                if (_connectionAttempts < 3)
                    await Task.Delay(500, _cancellationTokenSource.Token);
                else
                {
                    Console.WriteLine("Не удалось установить соединение после всех попыток");
                    break;
                }

                _connectionAttempts++;
            }
        }
    }

    private async Task StopReceivingMessagesAsync()
    {
        if (_receiveTask != null)
        {
            try
            {
                await _receiveTask;
            }
            catch (OperationCanceledException)
            {
                // Нормальное завершение
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при ожидании завершения задачи приема сообщений: {ex.Message}");
            }
        }
    }

    public async Task StopConnectionAsync()
    {
        _cancellationTokenSource.Cancel();

        await CleanupCurrentConnectionAsync();
        await StopReceivingMessagesAsync();

        IsOpen = false;
    }

    public async Task<bool> SendMessageAsync(string message)
    {
        if (_isDisposed || !IsOpen)
            return false;

        try
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            await _webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true,
                _cancellationTokenSource.Token);
            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка отправки: {ex.Message}");
            await HandleDisconnectionAsync();
            return false;
        }
    }

    public async Task StartReceivingMessagesAsync()
    {
        try
        {
            if (_webSocket.State != WebSocketState.Open)
                return;

            await ProcessMessagesUntilDisconnectAsync();
        }
        catch (OperationCanceledException)
        {
            // Нормальное завершение при отмене
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка приема сообщений: {ex.Message}");
            await HandleDisconnectionAsync();
        }
    }

    private async Task ProcessMessagesUntilDisconnectAsync()
    {
        byte[] buffer = new byte[1024 * 4];

        while (_webSocket.State == WebSocketState.Open && !_isDisposed &&
               !_cancellationTokenSource.Token.IsCancellationRequested)
        {
            var result = await ReceiveMessageAsync(buffer);
            if (result.IsConnectionClosed)
                break;

            if (result.Message != null && !string.IsNullOrEmpty(result.Message))
            {
                var messageReceivedHandler = MessageReceived;
                messageReceivedHandler?.Invoke(result.Message);
            }
        }
    }

    private async Task<(bool IsConnectionClosed, string? Message)> ReceiveMessageAsync(byte[] buffer)
    {
        try
        {
            WebSocketReceiveResult result;
            var messageBuffer = new List<byte>();

            do
            {
                result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationTokenSource.Token);
                if (result.MessageType == WebSocketMessageType.Close)
                    return (true, null);

                messageBuffer.AddRange(new ArraySegment<byte>(buffer, 0, result.Count));
            } while (!result.EndOfMessage);

            var message = Encoding.UTF8.GetString(messageBuffer.ToArray());
            return (false, message);
        }
        catch (OperationCanceledException)
        {
            return (true, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при получении сообщения: {ex.Message}");
            return (true, null);
        }
    }

    private async Task HandleDisconnectionAsync()
    {
        if (_isDisposed) return;

        await _connectionLock.WaitAsync(_cancellationTokenSource.Token);
        try
        {
            if (!IsOpen) return;

            IsOpen = false;
            Console.WriteLine("Соединение потеряно, переподключение...");
            await ReconnectAsync();
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    private async Task ReconnectAsync()
    {
        if (_isReconnecting) return;

        try
        {
            _isReconnecting = true;
            _connectionAttempts = 0;

            for (int attempt = 1;
                 attempt <= _maxReconnectAttempts && !_cancellationTokenSource.Token.IsCancellationRequested;
                 attempt++)
            {
                if (await TryReconnectOnceAsync(attempt)) return;

                if (attempt < _maxReconnectAttempts)
                    await Task.Delay(_reconnectDelay, _cancellationTokenSource.Token);
            }

            Console.WriteLine("Не удалось переподключиться после всех попыток");
        }
        finally
        {
            _isReconnecting = false;
        }
    }

    private async Task<bool> TryReconnectOnceAsync(int attempt)
    {
        try
        {
            await CleanupCurrentConnectionAsync();
            _webSocket = new ClientWebSocket();
            await _webSocket.ConnectAsync(_serverUri, _cancellationTokenSource.Token);
            IsOpen = true;

            _receiveTask = StartReceivingMessagesAsync();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Попытка переподключения {attempt} не удалась: {ex.Message}");
            return false;
        }
    }

    private async Task CleanupCurrentConnectionAsync()
    {
        if (_webSocket.State == WebSocketState.Open)
        {
            try
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Cleanup", CancellationToken.None);
            }
            catch
            {
                // Игнорируем ошибки при закрытии
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        _cancellationTokenSource.Cancel();

        try
        {
            if (_receiveTask != null)
                await _receiveTask;
        }
        catch
        {
            // Игнорируем ошибки при завершении задачи
        }

        await CleanupCurrentConnectionAsync();
        _cancellationTokenSource.Dispose();
        _connectionLock.Dispose();
    }
}
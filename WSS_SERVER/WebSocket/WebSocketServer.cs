using System.Net;

namespace WebSocketServer;

public class Server
{
    private readonly string _url;
    private readonly IClientManager _clientManager;
    private readonly IMessageHandler _messageHandler;
    private readonly IMessageArchive _messageArchive;
    
    public Server(string url, IClientManager clientManager, IMessageHandler messageHandler, IMessageArchive messageArchive)
    {
        _url = url;
        _clientManager = clientManager;
        _messageHandler = messageHandler;
        _messageArchive = messageArchive;
    }
    
    public async Task StartAsync()
    {
        _ = _messageArchive.StartMessageResendTimer();
        _ = _clientManager.StartConnectionHealthCheck();

        var httpListener = new HttpListener();
        httpListener.Prefixes.Add(_url);
        httpListener.Start();

        Console.WriteLine($"WebSocket-сервер запущен на {_url}");

        while (true)
        {
            var listenerContext = await httpListener.GetContextAsync();
            if (listenerContext.Request.IsWebSocketRequest)
                _ = Task.Run(async () => await ProcessWebSocketRequest(listenerContext));
            else
                HandleInvalidWebSocketRequest(listenerContext);
        }
    }

    private async Task ProcessWebSocketRequest(HttpListenerContext listenerContext)
    {
        try 
        {
            var webSocketContext = await listenerContext.AcceptWebSocketAsync(subProtocol: null);
            Console.WriteLine("Клиент подключен!");

            var webSocket = webSocketContext.WebSocket;
            await _messageHandler.HandleConnectionAsync(webSocket);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обработке WebSocket запроса: {ex.Message}");
            listenerContext.Response.StatusCode = 500;
            listenerContext.Response.Close();
        }
    }

    private void HandleInvalidWebSocketRequest(HttpListenerContext listenerContext)
    {
        listenerContext.Response.StatusCode = 400;
        listenerContext.Response.Close();
        //Console.WriteLine("Некорректный WebSocket-запрос.");
    }
}

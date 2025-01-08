using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using Domain;
using WebSocketServer;

public class ChatServer
{
    private static ConcurrentDictionary<int, WebSocket> _clients = new();
    private static Dictionary<int, HashSet<string>> _messagesToSend = new();
    private static readonly SemaphoreSlim _dictSamaphore = new(1, 1);

    static async Task Main()
    {
        var url = "http://localhost:8080/";
        var clientManager = new ClientManager();
        var messageArchive = new MessageArchive(clientManager);
        var messageHandler = new MessageHandler(clientManager, messageArchive);
        var server = new Server(url, clientManager, messageHandler, messageArchive);

        try
        {
            Console.WriteLine("Запуск WebSocket сервера...");
            await server.StartAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при запуске сервера: {ex.Message}");
        }
    }
}
using System.Net.WebSockets;

namespace WebSocketServer;

public interface IClientManager
{
    Task<bool> AddClient(int clientId, WebSocket webSocket);
    Task<bool> RemoveClient(int clientId);
    Task<bool> SendMessageToClientAsync(int receiverId, string message, int senderId);
    bool HasClient(int clientId);
    Task CleanupClientAsync(int clientId, WebSocket webSocket);
    Task CloseConnectionAsync(WebSocket webSocket);
    Task StopAsync();
    Task StartAsync();
}

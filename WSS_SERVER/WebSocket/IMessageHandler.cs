using System.Net.WebSockets;

namespace WebSocketServer;

public interface IMessageHandler
{
    Task HandleConnectionAsync(WebSocket webSocket);
    Task<string> ReceiveMessageAsync(WebSocket webSocket, byte[] buffer);
    Task<int> RegisterClient(string message, WebSocket webSocket);
    Task HandleClientMessageAsync(string receivedMessage, int senderId);
}

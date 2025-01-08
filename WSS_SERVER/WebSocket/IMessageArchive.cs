using Domain;

namespace WebSocketServer;

public interface IMessageArchive
{
    Task ArchiveMessage(ChatMessage message);
    Task SendArchivedMessages(int clientId);
    Task StartMessageResendTimer();
}

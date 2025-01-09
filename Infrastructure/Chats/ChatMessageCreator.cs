using System.Text.RegularExpressions;
using Domain;

namespace Infrastructure.Chats;

public static class ChatMessageFormatter
{
    public static string CreateMessage(int senderId, string message, int recieverId, string sendTime, string addInfo)
    {
        var formattedMessage = $"[CHATMSG][{senderId}][{message}][{recieverId}][{sendTime}][{addInfo}]";
        return formattedMessage;
    }

    public static string CreateMessage(ChatMessage message)
    {
        return CreateMessage(message.SenderId, message.MessageContext, message.RecieverId, message.MessageTime,
            message.AdditionalInfo);
    }

    public static ChatMessage ParseClientMessage(string message)
    {
        string pattern = $@"^\[CHATMSG\]\[(\d+)\]\[(.*?)\]\[(\d+)\]\[(.*?)\]\[(.*?)\]$";
        var match = Regex.Match(message, pattern);

        if (match.Success)
        {
            var senderId = int.Parse(match.Groups[1].Value);
            var messageText = match.Groups[2].Value;
            var receiverId = int.Parse(match.Groups[3].Value);
            var time = match.Groups[4].Value;
            var addInfo = match.Groups[5].Value;

            return new ChatMessage(
                senderId: senderId,
                messageContext: messageText,
                recieverId: receiverId,
                messageTimestamp: time,
                additionalInfo: addInfo);
        }

        return null;
    }
}
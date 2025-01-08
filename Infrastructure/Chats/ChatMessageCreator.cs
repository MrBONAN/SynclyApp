using System.Text.RegularExpressions;
using Domain;

namespace Infrastructure.Chats;

public static class ChatMessageFormatter
{
    public static string CreateMessage(int senderId, string message, int recieverId, string sendTime, string addInfo)
    {
        // [CHATMSG][from][says][to][time][info]
        var formattedMessage = $"[CHATMSG][{senderId}][{message}][{recieverId}][{sendTime}][{addInfo}]";
        return formattedMessage;
    }

    public static string CreateMessage(ChatMessage message)
    {
        return CreateMessage(message.SenderId, message.MessageContext, message.RecieverId, message.MessageTime, message.AdditionalInfo);
    }
    
    public static ChatMessage ParseClientMessage(string message)
    {
        // [CHATMSG][from][says][to][time][info]
        string pattern = $@"^\[CHATMSG\]\[(\d+)\]\[(.*?)\]\[(\d+)\]\[(.*?)\]\[(.*?)\]$";
        var match = Regex.Match(message, pattern);

        if (match.Success)
        {
            var senderId = int.Parse(match.Groups[1].Value);    // from
            var messageText = match.Groups[2].Value;            // says
            var receiverId = int.Parse(match.Groups[3].Value);  // to
            var time = match.Groups[4].Value;                   // time
            var addInfo = match.Groups[5].Value;                // info

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
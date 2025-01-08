using System.Text.RegularExpressions;

namespace Domain;

public static class ChatMessageFormatter
{
    public static ChatMessage ParseClientMessage(string message)
    {
        // [CHATMSG][senderId][message][receiverId][time][addInfo]
        string pattern = $@"^\[CHATMSG\]\[(\d+)\]\[(.*?)\]\[(\d+)\]\[(.*?)\]\[(.*?)\]$";
        var match = Regex.Match(message, pattern);

        if (match.Success)
        {
            return new ChatMessage(
                recieverId: int.Parse(match.Groups[3].Value),  // receiverId из группы 3
                messageContext: match.Groups[2].Value,         // message из группы 2
                senderId: int.Parse(match.Groups[1].Value),    // senderId из группы 1
                messageTimestamp: match.Groups[4].Value,       // time из группы 4
                additionalInfo: match.Groups[5].Value);        // addInfo из группы 5
        }

        return null;
    }

    public static string CreateMessage(int senderId, string message, int recieverId, string sendTime, string addInfo)
    {
        // [CHATMSG][senderId][message][receiverId][time][addInfo]
        var formattedMessage = $"[CHATMSG][{senderId}][{message}][{recieverId}][{sendTime}][{addInfo}]";
        return formattedMessage;
    }

    public static string CreateMessage(ChatMessage message) => CreateMessage(message.SenderId, message.MessageContext,
        message.RecieverId, message.MessageTime, message.AdditionalInfo);
}

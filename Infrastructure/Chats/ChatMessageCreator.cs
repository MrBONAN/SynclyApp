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
    
    public static ChatMessage ParseClientMessage(string message)
    {
        string pattern = $@"^\[CHATMSG\]\[(\d+)\]\[(.*?)\]\[(\d+)\]\[(.*?)\]\[(.*?)\]$";
        var match = Regex.Match(message, pattern);

        if (match.Success)
        {
            return new ChatMessage(
                int.Parse(match.Groups[1].Value), 
                match.Groups[2].Value,
                int.Parse(match.Groups[3].Value),
                match.Groups[4].Value,
                    match.Groups[5].Value);
        }

        return null;
    }
}
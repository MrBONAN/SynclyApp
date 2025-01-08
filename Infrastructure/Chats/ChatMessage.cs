namespace Domain;

public class ChatMessage
{
    public int SenderId { get; set; }
    public string MessageContext { get; set; }
    public int RecieverId { get; set; }
    
    public string MessageTime { get; set; }
    
    public string AdditionalInfo { get; set; }

    public ChatMessage(int senderId, string messageContext, int recieverId, string messageTimestamp, string additionalInfo)
    {
        SenderId = senderId;
        MessageContext = messageContext;
        RecieverId = recieverId;
        MessageTime = messageTimestamp;
        AdditionalInfo = additionalInfo;
    }
}
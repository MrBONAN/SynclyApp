namespace Domain;

public class ChatMessage
{
    public int RecieverId { get; set; }
    public string MessageContext { get; set; }
    public int SenderId { get; set; }
    
    public string MessageTime { get; set; }
    
    public string AdditionalInfo { get; set; }

    public ChatMessage(int recieverId, string messageContext, int senderId, string messageTimestamp, string additionalInfo)
    {
        RecieverId = recieverId;
        MessageContext = messageContext;
        SenderId = senderId;
        MessageTime = messageTimestamp;
        AdditionalInfo = additionalInfo;
    }
}
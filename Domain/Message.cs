using System.Windows.Input;
using Infrastructure.API.ServerApi.Models;

namespace Domain;

public class Message
{
    public int From { get; private set; }
    public int To { get; private set; }
    public string? MessageText { get; private set; }
    public DateTime UtcDate { get; private set; }
    public LayoutOptions Position { get; private set; }
    
    public Message(int from, int to, string? messageText, DateTime utcDate, LayoutOptions position)
    {
        From = from;
        To = to;
        MessageText = messageText;
        UtcDate = utcDate;
        Position = position;
    }
}
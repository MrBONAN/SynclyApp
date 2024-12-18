namespace Domain;

public class ProfileEventArgs : EventArgs
{
    public string Action { get; }
    public Dictionary<string, object> AdditionalData { get; }

    public ProfileEventArgs(string action = "", Dictionary<string, object> additionalData = null)
    {
        Action = action;
        AdditionalData = additionalData ?? new Dictionary<string, object>();
    }
}
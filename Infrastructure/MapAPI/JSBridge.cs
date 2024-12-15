using Android.Webkit;
using Java.Interop;

namespace App.Infrastructure;

public class JSBridge
{
    public event EventHandler ProfileClicked;

    [JavascriptInterface]
    [Export("invokeAction")]
    public void InvokeAction(string data)
    {
        ProfileClicked?.Invoke(new object(), new EventArgs());
    }
}
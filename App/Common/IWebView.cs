namespace App;

public interface IWebView
{
    void Eval(string js);
    WebViewSource Source { get; set; }
}

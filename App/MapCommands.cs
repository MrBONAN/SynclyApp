using App.Infrastructure;
using Infrastructure;

namespace App;

public class MapCommands
{
    private readonly WebView _leafletWebView;

    public MapCommands(WebView webView)
    {
        _leafletWebView = webView;
    }

    public void SetMapHtml(string htmlContent)
    {
        _leafletWebView.Source = new HtmlWebViewSource
        {
            Html = htmlContent
        };
    }

    public void MoveMapTo(Location location)
    {
        var jsCode = $"moveMap({location.Latitude}, {location.Longitude}, {12});";
        _leafletWebView.Eval(MapService.FormatJsCodeWithInvariantCulture(jsCode));
    }

    public void AddCircle(Location location, double radius)
    {
        var jsCode = $"addCircle({location.Latitude}, {location.Longitude}, {radius});";
        _leafletWebView.Eval(MapService.FormatJsCodeWithInvariantCulture(jsCode));
    }

    public void AddMarkerWithLocalImage(Location location, string imagePath, int id, string onClickFunc)
    {
        string onClickJs = $"function() {{ {onClickFunc}('{id}'); }}";
        var jsCode = $@"
        addUserMarker(
            {location.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}, 
            {location.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}, 
            '{imagePath}', 
            {id}, 
            {onClickJs}
        );";
        _leafletWebView.Eval(jsCode);
    }

    public void SetPort(PortChecker portChecker)
    {
        var port = portChecker.GetFreePort();
        var jsCode = $"setPort({port});";
        _leafletWebView.Eval(MapService.FormatJsCodeWithInvariantCulture(jsCode));
    }
}
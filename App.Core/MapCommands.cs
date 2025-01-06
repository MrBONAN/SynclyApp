using Domain;
using Infrastructure;

namespace App.Core;

public class MapCommands : IMapService
{
    private readonly IWebViewService _webView;

    public MapCommands(IWebViewService webView)
    {
        _webView = webView;
    }

    public void MoveMapTo(Location location)
    {
        var jsCode = $"moveMap({location.Latitude}, {location.Longitude}, {12});";
        _webView.Eval(MapService.FormatJsCodeWithInvariantCulture(jsCode));
    }

    public void AddCircle(Location location, double radius)
    {
        var jsCode = $"addCircle({location.Latitude}, {location.Longitude}, {radius});";
        _webView.Eval(MapService.FormatJsCodeWithInvariantCulture(jsCode));
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
        _webView.Eval(jsCode);
    }

    public void SetPort(int port)
    {
        var jsCode = $"setPort({port}); console.log('SetPort called with port:', {port});";
        System.Diagnostics.Debug.WriteLine($"Calling SetPort with port: {port}");
        _webView.Eval(MapService.FormatJsCodeWithInvariantCulture(jsCode));
    }
}

namespace App;

using Domain;
using Infrastructure;

public partial class MapPage : ContentPage
{
    private bool _isCheckingLocation;
    private UserInformation _userInformation;
    private MapLocation _cachedLocation;
    private MapCommands _mapControl;
    private DefaultSettings _defaultSettings;

    public MapPage()
    {
        InitializeComponent();
        InitializeFields();
    }

    private async void InitializeFields()
    {
        _mapControl = new MapCommands(LeafletWebView);
        _userInformation = new UserInformation();
        _defaultSettings = new DefaultSettings();
        _cachedLocation = new MapLocation(_userInformation.GetCurrentLocation);
        var htmlContent = _defaultSettings.GetMapHtml();
        _mapControl.SetMapHtml(htmlContent);
    }

    protected override async void OnAppearing() => base.OnAppearing();

    private async void OnClickedMoveToMyLocation(object sender, EventArgs e)
    {
        if (!_isCheckingLocation)
            try
            {
                _isCheckingLocation = true;
                _mapControl.MoveMapTo(await _cachedLocation.GetLocationAsync());
                _mapControl.AddCircle(await _cachedLocation.GetLocationAsync(), 2000);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                _isCheckingLocation = false;
            }
    }

    private async void OnClickedShowRocksi(object sender, EventArgs e)
    {
        var id = 1;
        _mapControl.AddMarkerWithLocalImage(await _cachedLocation.GetLocationAsync(), "image.jpg", id, "openUserProfile");
    }
}

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

    //ToDo: Я хз какой путь тут стоит указывать и как оно относится к БД
    public void AddMarkerWithLocalImage(Location location, string imagePath, int id, string onClickFunc)
    {
        string onClickJs = $"function() {{ {onClickFunc}('{id}'); }}";

        var jsCode = $"addUserMarker({location.Latitude}, {location.Longitude}, '{imagePath}', {id}, {onClickJs});";
        _leafletWebView.Eval(MapService.FormatJsCodeWithInvariantCulture(jsCode));
    }

}

public class UserInformation
{
    public async Task<Location> GetCurrentLocation()
    {
        GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(5));
        var cancelTokenSource = new CancellationTokenSource();
        var newLocation = await Geolocation.Default.GetLocationAsync(request, cancelTokenSource.Token);
        if (newLocation == null)
            throw new Exception("fatal error");

        return newLocation;
    }
}

public class DefaultSettings
{
    private string _mapSource = "Map.html";

    public string GetMapHtml()
    {
        using var stream = FileSystem.OpenAppPackageFileAsync(_mapSource).Result;
        using var reader = new StreamReader(stream);

        var htmlContent = reader.ReadToEndAsync().Result;
        return htmlContent;
    }
}
namespace App;

//ToDo: MOVE TO DOMAIN
public class MapLocation
{
    private readonly Func<Task<Location>> _locationGetter;
    private Location _location;
    private DateTimeOffset _timeWhenUpdated;

    public MapLocation(Func<Task<Location>> getLocation)
    {
        _locationGetter = getLocation ?? throw new ArgumentNullException(nameof(getLocation));
        _timeWhenUpdated = DateTimeOffset.MinValue;
    }

    public async Task<Location> GetLocationAsync()
    {
        if (DateTimeOffset.Now - _timeWhenUpdated > TimeSpan.FromSeconds(10))
        {
            _location = await _locationGetter();
            _timeWhenUpdated = DateTimeOffset.Now;
        }

        return _location;
    }

    public void SetLocation(Location location)
    {
        _location = location;
        _timeWhenUpdated = DateTimeOffset.Now;
    }
}

public partial class MapPage : ContentPage
{
    private bool _isCheckingLocation = false;
    private MapLocation _cachedLocation = new MapLocation(GetCurrentLocation);

    public MapPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var htmlContent = await GetHtmlAsync();
        SetMapHtml(htmlContent);
    }

    private async void OnClickedMoveToMyLocation(object sender, EventArgs e)
    {
        if (!_isCheckingLocation)
            try
            {
                _isCheckingLocation = true;
                MoveMapTo(await _cachedLocation.GetLocationAsync());
                AddCircle(await _cachedLocation.GetLocationAsync(), 2000);
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

    public static async Task<Location> GetCurrentLocation()
    {
        GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.Medium);
        var cancelTokenSource = new CancellationTokenSource();
        Location? newLocation = await Geolocation.Default.GetLocationAsync(request, cancelTokenSource.Token);
        if (newLocation == null)
            throw new Exception("fatal error");

        return newLocation;
    }

    private async Task<string> GetHtmlAsync()
    {
        using var stream = await FileSystem.OpenAppPackageFileAsync("Map.html");
        using var reader = new StreamReader(stream);

        var htmlContent = reader.ReadToEndAsync();
        return await htmlContent;
    }

    private async void OnClickedShowRocksi(object sender, EventArgs e)
    {
        AddMarkerWithLocalImage(await GetCurrentLocation(), "image.jpg");
    }

    private void SetMapHtml(string htmlContent)
    {
        LeafletWebView.Source = new HtmlWebViewSource
        {
            Html = htmlContent
        };
    }

    private void MoveMapTo(Location location)
    {
        var jsCode = $"moveMap({location.Latitude}, {location.Longitude}, {12});";
        LeafletWebView.Eval(jsCode);
    }

    private void AddCircle(Location location, double radius)
    {
        var jsCode = $"addCircle({location.Latitude}, {location.Longitude}, {radius});";
        LeafletWebView.Eval(jsCode);
    }

    //ToDo: Я хз какой путь тут стоит указывать и как оно относится к БД
    private void AddMarkerWithLocalImage(Location location, string imagePath)
    {
        var jsCode = $"addMarkerWithLocalImage({location.Latitude}, {location.Longitude}, '{imagePath}');";
        LeafletWebView.Eval(jsCode);
    }
}
using System.ComponentModel;
using Domain;
using App.Infrastructure;
using CommunityToolkit.Maui.Core.Views;
using CommunityToolkit.Maui.Views;
using Infrastructure.API.SpotifyAPI;
using Microsoft.Maui.Controls;
using ProfileBottomSheet;
using The49.Maui.BottomSheet;
namespace App;

public partial class Map : ContentPage, INotifyPropertyChanged
{
    private bool _isCheckingLocation;
    private UserInformation _userInformation;
    private MapLocation _cachedLocation;
    private MapCommands _mapControl;
    private DefaultSettings _defaultSettings;

    private CancellationTokenSource _cancelTokenSource;
    private string _topText = "None";

    public string TopText
    {
        get => _topText;
        set
        {
            if (_topText == value) return;
            _topText = value;
            OnPropertyChanged();
        }
    }

    public Map()
    {
        InitializeComponent();
        InitializeFields();
        
        BindingContext = this;
        ProfileButton.Clicked += OnProfileButtonClicked;
        SettingsButton.Clicked += OnSettingsButtonClicked;
        ActionButton.Clicked += OnClickedMoveToMyLocation;
        // GetCurrentLocation().RunSynchronously();
        // _jsBridge = new JSBridge();
        // _jsBridge.ProfileClicked += OnProfileButtonClicked;
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

    protected async override void OnAppearing()
    {
        base.OnAppearing();

        using var stream = await FileSystem.OpenAppPackageFileAsync("Map.html");
        using var reader = new StreamReader(stream);
        var htmlContent = reader.ReadToEnd();
        LeafletWebView.Source = new HtmlWebViewSource
        {
            Html = htmlContent
        };
    }

    public async Task GetCurrentLocation()
    {
        try

        {
            _isCheckingLocation = true;
            GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(5));
            _cancelTokenSource = new CancellationTokenSource();
            Location location = await Geolocation.Default.GetLocationAsync(request, _cancelTokenSource.Token);

            if (location != null)
                DisplayAlert("Success",
                    $"Широта: {location.Latitude}, Долгота: {location.Longitude}, Altitude: {location.Altitude}", "OK");
        }
        catch (Exception ex)
        {
            DisplayAlert("Warning", "Location could not be retrieved: " + ex.Message, "OK");
        }
        finally
        {
            _isCheckingLocation = false;
        }
    }
    
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

    private async void ShowProfileOnMap(int id)
    {
        _mapControl.AddMarkerWithLocalImage(await _cachedLocation.GetLocationAsync(), "image.jpg", id, "openUserProfile");
    }
    
    private async Task<string> UpdateTopText(string text)
    {
        throw new NotImplementedException();
    }
    
    private async void OnProfileButtonClicked(object sender, EventArgs e)
    {
        var page = new Sheet();
        await page.ShowAsync();
    }
    
    private async void OnSettingsButtonClicked(object sender, EventArgs e)
    {
        var page = new Settings();
        await page.ShowAsync();
    }
    
    private void OnBottomButtonClicked(object sender, EventArgs e)
    {
        //OnClickedMoveToMyLocation(sender, e);
    }

    private void OnWebViewNavigated(object? sender, WebNavigatedEventArgs e)
    {
        throw new NotImplementedException();
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


}

public class UserInformation
{
    public async Task<Location> GetCurrentLocation()
    {
        //var isGranted = PermissionStatus.Unknown;
        //while (isGranted != PermissionStatus.Granted)
        //{
        //    isGranted = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        //}
        var cachedLocation = await Geolocation.Default.GetLastKnownLocationAsync();
        if (cachedLocation != null)
            return cachedLocation;
        
        var request = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(10));
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
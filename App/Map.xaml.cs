using Domain;
using App.UserAuthorization;
using Infrastructure;
using Infrastructure.API.ServerApi;
using Infrastructure.API.ServerApi.Models.Location;
using Infrastructure.API.ServerApi.Models.User;
using ProfileBottomSheet;
using ApiResult = Infrastructure.API.ServerApi.ApiResult;

namespace App;

public partial class Map : ContentPage
{
    private bool _isCheckingLocation;
    private UserInformation _userInformation;
    private MapLocation _cachedLocation;
    private MapCommands _mapControl;
    private DefaultSettings _defaultSettings;
    private SimpleServer _localServer;
    private PortChecker _portChecker;
    private UserDto userData;
    private string MapStyle => Preferences.Get("MapStyle", "default");
    private readonly IUserDataHandler userDataHandler = App.Services.GetRequiredService<IUserDataHandler>();
    private string _topText = "Тишина...";
    private string _topTextLink;

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

    public string TopTextLink
    {
        get => _topTextLink;
        set
        {
            if (_topTextLink == value) return;
            _topTextLink = value;
            OnPropertyChanged();
        }
    }

    public Map()
    {
        InitializeComponent();
        StartServer();
        InitializeFields();
        HandleXamlButtons();
        Loaded += OnPageLoaded;
    }

    private async Task UpdateLocation()
    {
        while (true)
        {
            try
            {
                var location = await _cachedLocation.GetLocationAsync();
                await ServerApi.UpdateLocationAsync(userData.Id,
                    new UpdateLocationDto
                    {
                        Latitude = (decimal)location.Latitude,
                        Longitude = (decimal)location.Longitude
                    });
                if (location != null)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        _mapControl.AddMarkerWithLocalImage(location, userData.Links.ExternalImageLink, userData.Id,
                            "openUserProfile");
                        _mapControl.SetPort(_portChecker);
                    });
                }
                else
                {
                    Console.WriteLine("UpdateLocation: Location is null");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateLocation error: {ex.Message}");
            }

            await Task.Delay(15000);
        }
    }

    private void HandleXamlButtons()
    {
        BindingContext = this;
        ProfileButton.Clicked += OnProfileButtonClicked;
        SettingsButton.Clicked += OnSettingsButtonClicked;
        ActionButton.Clicked += OnClickedMoveToMyLocation;
        ChatButton.Clicked += OnChatButtonClicked;
    }

    private async void InitializeFields()
    {
        _userInformation = new UserInformation();
        userData = await userDataHandler.GetUserDataAsync();
        _defaultSettings = new DefaultSettings();

        _mapControl = App.Services.GetRequiredService<MapCommands>();
        _mapControl.Initialize(LeafletWebView);
        _mapControl.SetMapHtml(_defaultSettings.GetMapHtml());

        _cachedLocation = new MapLocation(_userInformation.GetCurrentLocation);
        HandleServerMethods();
        await UpdateTopText();
    }

    private void StopServer() => _localServer.Stop();

    private void StartServer()
    {
        _portChecker = new PortChecker();
        _localServer = new SimpleServer(_portChecker);
        _localServer.Start();
    }

    private void HandleServerMethods()
    {
        _localServer.AddHandler("OpenUserProfile", OnServerProfileRequest);
    }

    protected async override void OnAppearing() => base.OnAppearing();

    private async void OnClickedMoveToMyLocation(object sender, EventArgs e)
    {
        if (_isCheckingLocation) return;
        try
        {
            _isCheckingLocation = true;
            var userLocation = await _cachedLocation.GetLocationAsync();
            _mapControl.MoveMapTo(userLocation);
            _mapControl.AddCircle(await _cachedLocation.GetLocationAsync(), 2000);
            var allLocations = (await ServerApi.GetAllLocationsAsync()).Data;
            foreach (var location in allLocations)
            {
                var user = (await ServerApi.GetUserAsync(location.UserId)).Data;
                var locationUser = new Location()
                {
                    Latitude = (double)location.Latitude,
                    Longitude= (double)location.Longitude
                };
                _mapControl.AddMarkerWithLocalImage(locationUser, user.Links.ExternalImageLink, location.UserId, "openUserProfile");
            }
            _mapControl.SetPort(_portChecker);
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

    private async Task UpdateTopText()
    {
        while (true)
        {
            var currentTrack = await ServerApi.GetCurrentTrackAsync(userData.Id);
            if (currentTrack.Result is ApiResult.Ok)
            {
                TopText = currentTrack.Data!.Name;
                TopTextLink = currentTrack.Data!.Links.ExternalImageLink;
            }
            else
            {
                TopText = "Тишина...";
                TopTextLink = null;
            }

            await Task.Delay(10000);
        }
    }

    private async void OnWebViewNavigated(object sender, EventArgs e)
    {
        var userLocation = await _cachedLocation.GetLocationAsync();
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            _mapControl.LoadMap();
            _mapControl.MoveMapTo(userLocation);
            _mapControl.AddMarkerWithLocalImage(userLocation, userData.Links.ExternalImageLink, userData.Id, "openUserProfile");
            _mapControl.SetPort(_portChecker);
        });
    }

    private async void OnProfileButtonClicked(object sender, EventArgs e)
    {
        var page = new Sheet(userData.Id);
        await page.ShowAsync();
    }

    private async void OnSettingsButtonClicked(object sender, EventArgs e)
    {
        var page = new SettingsBottomSheet();
        await page.ShowAsync();
    }

    private async void OnChatButtonClicked(object? sender, EventArgs e)
    {
        var page = new Chat.Sheet(0);
        await page.ShowAsync();
    }

    private async void OpenUserProfile(int id)
    {
        var page = new Sheet(id);
        await page.ShowAsync();
    }

    private async void OnServerProfileRequest(object sender, EventArgs e)
    {
        if (e is ProfileEventArgs)
        {
            var args = (ProfileEventArgs)e;
            var id = Convert.ToInt16(args.AdditionalData["id"]);
            OpenUserProfile(id);
        }
    }

    private async void OnTopTextTapped(object? sender, TappedEventArgs e)
    {
        if (TopTextLink != null)
            await Launcher.OpenAsync(TopTextLink);
    }

    private async void OnPageLoaded(object sender, EventArgs e)
    {
        try
        {
            await Task.Delay(1000);
            var userLocation = await _cachedLocation.GetLocationAsync();

            var image = userData.Links.ExternalImageLink;
            var id = userData.Id;

            if (userLocation != null)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    _mapControl.LoadMap();
                    _mapControl.MoveMapTo(userLocation);
                    _mapControl.AddMarkerWithLocalImage(userLocation, image, id, "openUserProfile");
                    _mapControl.SetPort(_portChecker);
                });
            }

            _ = UpdateLocation();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error in OnPageLoaded", ex.Message, "OK");
        }
    }
}
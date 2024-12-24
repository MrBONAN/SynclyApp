using System.ComponentModel;
using Domain;
using App.Infrastructure;
using App.UserAuthorization.SpotifyAuthorization;
using App.UserAuthorization.SpotifyAuthorization.Models;
using CommunityToolkit.Maui.Core.Views;
using CommunityToolkit.Maui.Views;
using Infrastructure;
using Infrastructure.API.SpotifyAPI;
using Microsoft.Maui.Controls;
using ProfileBottomSheet;
using The49.Maui.BottomSheet;

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

    private readonly ISpotifyAccessTokenService spotifyAccessToken;
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

    public Map(ISpotifyAccessTokenService spotifyAccessToken)
    {
        this.spotifyAccessToken = spotifyAccessToken;
        InitializeComponent();
        StartServer();
        InitializeFields();
        HandleXamlButtons();
        this.Loaded += OnPageLoaded;
    }

    private void HandleXamlButtons()
    {
        BindingContext = this;
        ProfileButton.Clicked += OnProfileButtonClicked;
        SettingsButton.Clicked += OnSettingsButtonClicked;
        ActionButton.Clicked += OnClickedMoveToMyLocation;
    }

    private async void InitializeFields()
    {
        _userInformation = new UserInformation();
        _defaultSettings = new DefaultSettings();
        _mapControl = new MapCommands(LeafletWebView);
        _mapControl.SetMapHtml(_defaultSettings.GetMapHtml());
        _cachedLocation = new MapLocation(_userInformation.GetCurrentLocation);
        HandleServerMethods();
        await UpdateTopText();
        LeafletWebView.Navigated += OnWebViewNavigated;
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
            
            var locations = new List<Location>
            {
                userLocation,
                new Location(userLocation.Latitude + 0.015, userLocation.Longitude),
                new Location(userLocation.Latitude - 0.01, userLocation.Longitude - 0.01),
                new Location(userLocation.Latitude - 0.017, userLocation.Longitude - 0.002)
            };
            for (var i = 0; i < locations.Count; i++)
                _mapControl.AddMarkerWithLocalImage(locations[i], "image.jpg", i,
                    "openUserProfile");
            
            _mapControl.AddCircle(await _cachedLocation.GetLocationAsync(), 2000);
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
        var token = await spotifyAccessToken.GetAsync();
        while (token != null)
        {
            var currentTrack = await SpotifyApi.GetCurrentTrackAsync(token.Value!);
            if (currentTrack.Result is ApiResult.Success)
            {
                TopText = currentTrack.Data!.Name;
                TopTextLink = currentTrack.Data!.Uri;
            }

            await Task.Delay(10000);
        }
    }

    private async void OnWebViewNavigated(object sender, EventArgs e)
    {
        var userLocation = await _cachedLocation.GetLocationAsync();
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            _mapControl.MoveMapTo(userLocation);
            _mapControl.AddMarkerWithLocalImage(userLocation, "image.jpg", 0, "openUserProfile");
            _mapControl.SetPort(_portChecker);
        });
    }

    private async void OnProfileButtonClicked(object sender, EventArgs e)
    {
        var page = new Sheet(spotifyAccessToken, 0);
        await page.ShowAsync();
    }

    private async void OnSettingsButtonClicked(object sender, EventArgs e)
    {
        var page = new SettingsBottomSheet();
        await page.ShowAsync();
    }

    private void OnBottomButtonClicked(object sender, EventArgs e)
    {
        //OnClickedMoveToMyLocation(sender, e);
    }

    private async void OpenUserProfile(int id)
    {
        var page = new Sheet(spotifyAccessToken, id);
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
            if (userLocation != null)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    _mapControl.MoveMapTo(userLocation);
                    _mapControl.AddMarkerWithLocalImage(userLocation, "image.jpg", 0, "openUserProfile");
                    _mapControl.SetPort(_portChecker);
                });
            }
            else
            {
                await DisplayAlert("Debug", "Location is null", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error in OnPageLoaded", ex.Message, "OK");
        }
    }
}
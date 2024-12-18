using System.ComponentModel;
using Domain;
using App.Infrastructure;
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
        StartServer();
        InitializeFields();
        HandleXamlButtons();
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
        if (!_isCheckingLocation)
            try
            {
                _isCheckingLocation = true;
                _mapControl.AddMarkerWithLocalImage(await _cachedLocation.GetLocationAsync(), "image.jpg", 1,
                    "openUserProfile");
                _mapControl.MoveMapTo(await _cachedLocation.GetLocationAsync());
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

    private async Task<string> UpdateTopText(string text)
    {
        throw new NotImplementedException();
    }

    private async void OnWebViewNavigated(object sender, EventArgs e)
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
        OpenUserProfile(-1);
    }

    private void OnBottomButtonClicked(object sender, EventArgs e)
    {
        //OnClickedMoveToMyLocation(sender, e);
    }

    private async void OpenUserProfile(int id)
    {
        var page = new Sheet();
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
}
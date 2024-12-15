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
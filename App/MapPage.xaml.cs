using Infrastructure;

namespace App;

public partial class MapPage : ContentPage
{
    private CancellationTokenSource _cancelTokenSource;
    private bool _isCheckingLocation;

    public MapPage()
    {
        InitializeComponent();
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

    protected async void OnClickedMoveToMyLocation(object sender, EventArgs e)
    {
        GetCurrentLocation();
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
                DisplayAlert("Success", $"Широта: {location.Latitude}, Долгота: {location.Longitude}, Altitude: {location.Altitude}", "OK");
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
}
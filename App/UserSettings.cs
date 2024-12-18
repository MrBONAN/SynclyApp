namespace App;

public class UserInformation
{
    public async Task<Location> GetCurrentLocation()
    {
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
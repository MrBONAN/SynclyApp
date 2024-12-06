using Microsoft.Maui.Devices.Sensors;

namespace Domain;

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
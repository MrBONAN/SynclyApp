namespace Domain;

public interface ILocationService
{
    Task<Location> GetLocationAsync(int id);
    Task<IEnumerable<Location>> GetAllLocationsAsync();
    Task AddLocationAsync(Location location);
    Task UpdateLocationAsync(Location location);
    Task DeleteLocationAsync(int id);
}
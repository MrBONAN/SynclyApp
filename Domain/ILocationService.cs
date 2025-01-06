namespace Domain;

public interface ILocationService
{
    Task<IEnumerable<Location>> GetAllLocationsAsync();
    Task AddLocationAsync(Location location);
    Task UpdateLocationAsync(Location location);
    Task DeleteLocationAsync(int id);
}
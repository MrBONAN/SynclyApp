using Domain;

namespace Infrastructure;

public interface ILocationService
{
    Task<Location> GetLocationAsync();
}

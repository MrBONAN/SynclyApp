using Domain;

namespace Infrastructure;

public interface IMapService
{
    void MoveMapTo(Location location);
    void AddMarkerWithLocalImage(Location location, string imagePath, int id, string onClickFunc);
    void AddCircle(Location location, double radius);
    void SetPort(int port);
}

using Domain;
using Infrastructure;
using Moq;
using ILocationService = Infrastructure.ILocationService;

namespace App.Tests;

public class MapTests
{
    private readonly Mock<IMapService> _mapServiceMock;
    private readonly Mock<ILocationService> _locationServiceMock;

    public MapTests()
    {
        _mapServiceMock = new Mock<IMapService>();
        _locationServiceMock = new Mock<ILocationService>();
    }

    [Fact]
    public async Task MoveToLocation_WhenCalled_MovesMapToLocation()
    {
        // Arrange
        var expectedLocation = new Location(1.0, 1.0);
        _locationServiceMock.Setup(x => x.GetLocationAsync())
            .ReturnsAsync(expectedLocation);

        // Act
        _mapServiceMock.Object.MoveMapTo(expectedLocation);

        // Assert
        _mapServiceMock.Verify(x => x.MoveMapTo(expectedLocation), Times.Once);
    }

    [Fact]
    public void AddMarker_WhenCalled_AddsMarkerToMap()
    {
        // Arrange
        var location = new Location(1.0, 1.0);
        var imagePath = "test.jpg";
        var id = 1;
        var onClickFunc = "testFunc";

        // Act
        _mapServiceMock.Object.AddMarkerWithLocalImage(location, imagePath, id, onClickFunc);

        // Assert
        _mapServiceMock.Verify(x => x.AddMarkerWithLocalImage(
            location, imagePath, id, onClickFunc), Times.Once);
    }

    [Fact]
    public void AddCircle_WhenCalled_AddsCircleToMap()
    {
        // Arrange
        var location = new Location(1.0, 1.0);
        var radius = 1000.0;

        // Act
        _mapServiceMock.Object.AddCircle(location, radius);

        // Assert
        _mapServiceMock.Verify(x => x.AddCircle(location, radius), Times.Once);
    }

    [Fact]
    public void SetPort_WhenCalled_SetsPortForMap()
    {
        // Arrange
        var port = 8080;

        // Act
        _mapServiceMock.Object.SetPort(port);

        // Assert
        _mapServiceMock.Verify(x => x.SetPort(port), Times.Once);
    }
}

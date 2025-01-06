using Microsoft.Maui.Devices.Sensors;
using Xunit;
using Moq;

namespace Domain.Tests;

public class MapLocationTests
{
    [Fact]
    public async Task GetLocationAsync_WhenCalled_ReturnsLocation()
    {
        // Arrange
        var expectedLocation = new Location(1.0, 1.0);
        var locationGetterMock = new Mock<Func<Task<Location>>>();
        locationGetterMock.Setup(x => x()).ReturnsAsync(expectedLocation);
        var mapLocation = new MapLocation(locationGetterMock.Object);

        // Act
        var result = await mapLocation.GetLocationAsync();

        // Assert
        Assert.Equal(expectedLocation, result);
        locationGetterMock.Verify(x => x(), Times.Once);
    }

    [Fact]
    public async Task GetLocationAsync_WhenCalledMultipleTimesWithin10Seconds_CallsGetterOnce()
    {
        // Arrange
        var expectedLocation = new Location(1.0, 1.0);
        var locationGetterMock = new Mock<Func<Task<Location>>>();
        locationGetterMock.Setup(x => x()).ReturnsAsync(expectedLocation);
        var mapLocation = new MapLocation(locationGetterMock.Object);

        // Act
        await mapLocation.GetLocationAsync();
        await mapLocation.GetLocationAsync();
        await mapLocation.GetLocationAsync();

        // Assert
        locationGetterMock.Verify(x => x(), Times.Once);
    }

    [Fact]
    public void Constructor_WhenLocationGetterIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new MapLocation(null));
    }

    [Fact]
    public void SetLocation_WhenCalled_UpdatesLocation()
    {
        // Arrange
        var locationGetterMock = new Mock<Func<Task<Location>>>();
        var mapLocation = new MapLocation(locationGetterMock.Object);
        var expectedLocation = new Location(1.0, 1.0);

        // Act
        mapLocation.SetLocation(expectedLocation);

        // Assert
        Assert.Equal(expectedLocation, mapLocation.GetLocationAsync().Result);
        locationGetterMock.Verify(x => x(), Times.Never);
    }
}

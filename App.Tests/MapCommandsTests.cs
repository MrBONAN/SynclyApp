using Domain;
using Infrastructure;
using App;
using Microsoft.Maui.Controls;
using Moq;

namespace App.Tests;

public class MapCommandsTests
{
    private readonly Mock<IWebView> _webViewMock;
    private readonly Mock<ILocationService> _locationServiceMock;
    private readonly MapCommands _mapCommands;

    public MapCommandsTests()
    {
        _webViewMock = new Mock<IWebView>();
        _locationServiceMock = new Mock<ILocationService>();
        _mapCommands = new MapCommands(_webViewMock.Object);
    }

    [Fact]
    public async Task MoveToMyLocation_WhenCalled_MovesMapToCurrentLocation()
    {
        // Arrange
        var expectedLocation = new Location(1.0, 1.0);
        _locationServiceMock.Setup(x => x.GetLocationAsync())
            .ReturnsAsync(expectedLocation);

        // Act
        await _mapCommands.MoveToMyLocation(_locationServiceMock.Object);

        // Assert
        _webViewMock.Verify(w => w.Eval(It.Is<string>(s => 
            s.Contains("moveMap") && 
            s.Contains(expectedLocation.Latitude.ToString()) && 
            s.Contains(expectedLocation.Longitude.ToString()))));
    }

    [Fact]
    public void MoveMapTo_WhenCalled_ExecutesCorrectJavaScript()
    {
        // Arrange
        var location = new Location(1.0, 2.0);

        // Act
        _mapCommands.MoveMapTo(location);

        // Assert
        _webViewMock.Verify(w => w.Eval(It.Is<string>(s => 
            s.Contains("moveMap") && 
            s.Contains(location.Latitude.ToString()) && 
            s.Contains(location.Longitude.ToString()))));
    }

    [Fact]
    public void AddCircle_WhenCalled_ExecutesCorrectJavaScript()
    {
        // Arrange
        var location = new Location(1.0, 2.0);
        var radius = 1000.0;

        // Act
        _mapCommands.AddCircle(location, radius);

        // Assert
        _webViewMock.Verify(w => w.Eval(It.Is<string>(s => 
            s.Contains("addCircle") && 
            s.Contains(location.Latitude.ToString()) && 
            s.Contains(location.Longitude.ToString()) && 
            s.Contains(radius.ToString()))));
    }

    [Fact]
    public void AddMarkerWithLocalImage_WhenCalled_ExecutesCorrectJavaScript()
    {
        // Arrange
        var location = new Location(1.0, 2.0);
        var imagePath = "test.jpg";
        var id = 1;
        var onClickFunc = "testFunc";

        // Act
        _mapCommands.AddMarkerWithLocalImage(location, imagePath, id, onClickFunc);

        // Assert
        _webViewMock.Verify(w => w.Eval(It.Is<string>(s => 
            s.Contains("addUserMarker") && 
            s.Contains(location.Latitude.ToString()) && 
            s.Contains(location.Longitude.ToString()) && 
            s.Contains(imagePath) && 
            s.Contains(id.ToString()) && 
            s.Contains(onClickFunc))));
    }

    [Fact]
    public void SetPort_WhenCalled_ExecutesCorrectJavaScript()
    {
        // Arrange
        var port = 8080;

        // Act
        _mapCommands.SetPort(port);

        // Assert
        _webViewMock.Verify(w => w.Eval(It.Is<string>(s => 
            s.Contains("setPort") && 
            s.Contains(port.ToString()))));
    }
}

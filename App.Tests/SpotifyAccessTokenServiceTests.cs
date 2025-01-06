using App.UserAuthorization.SpotifyAuthorization;
using App.UserAuthorization.SpotifyAuthorization.Models;
using Moq;
using Xunit;

namespace App.Tests;

public class SpotifyAccessTokenServiceTests
{
    private readonly Mock<ISpotifyAuthorizationService> _authServiceMock;
    private readonly SpotifyAccessTokenService _tokenService;

    public SpotifyAccessTokenServiceTests()
    {
        _authServiceMock = new Mock<ISpotifyAuthorizationService>();
        _tokenService = new SpotifyAccessTokenService(_authServiceMock.Object);
    }

    [Fact]
    public async Task GetAsync_WhenTokenExists_ReturnsToken()
    {
        // Arrange
        var expectedToken = new SpotifyAccessToken { AccessToken = "test_token" };
        _authServiceMock.Setup(x => x.GetAccessTokenAsync())
            .ReturnsAsync(expectedToken);

        // Act
        var result = await _tokenService.GetAsync();

        // Assert
        Assert.Equal(expectedToken, result);
        _authServiceMock.Verify(x => x.GetAccessTokenAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAsync_WhenTokenDoesNotExist_ReturnsNull()
    {
        // Arrange
        _authServiceMock.Setup(x => x.GetAccessTokenAsync())
            .ReturnsAsync((SpotifyAccessToken)null);

        // Act
        var result = await _tokenService.GetAsync();

        // Assert
        Assert.Null(result);
        _authServiceMock.Verify(x => x.GetAccessTokenAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAsync_WhenExceptionOccurs_ReturnsNull()
    {
        // Arrange
        _authServiceMock.Setup(x => x.GetAccessTokenAsync())
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _tokenService.GetAsync();

        // Assert
        Assert.Null(result);
        _authServiceMock.Verify(x => x.GetAccessTokenAsync(), Times.Once);
    }
}

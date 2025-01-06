using App.UserAuthorization.SpotifyAuthorization.Models;
using Infrastructure.API.SpotifyAPI;
using Moq;
using Moq.Protected;
using System.Net;

namespace App.Tests;

public class SpotifyApiTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;

    public SpotifyApiTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
    }

    [Fact]
    public async Task GetCurrentTrackAsync_WhenSuccessful_ReturnsTrack()
    {
        // Arrange
        var token = new SpotifyAccessToken { AccessToken = "test_token" };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(@"{
                ""item"": {
                    ""id"": ""123"",
                    ""name"": ""Test Track"",
                    ""artists"": [
                        {
                            ""id"": ""456"",
                            ""name"": ""Test Artist""
                        }
                    ],
                    ""uri"": ""spotify:track:123""
                }
            }")
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        // Act
        var result = await SpotifyApi.GetCurrentTrackAsync(token, _httpClient);

        // Assert
        Assert.Equal(ApiResult.Success, result.Result);
        Assert.NotNull(result.Data);
        Assert.Equal("Test Track", result.Data.Name);
        Assert.Equal("spotify:track:123", result.Data.Uri);
    }

    [Fact]
    public async Task GetCurrentTrackAsync_WhenError_ReturnsError()
    {
        // Arrange
        var token = new SpotifyAccessToken { AccessToken = "test_token" };
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        // Act
        var result = await SpotifyApi.GetCurrentTrackAsync(token, _httpClient);

        // Assert
        Assert.Equal(ApiResult.Error, result.Result);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetCurrentTrackAsync_WhenNoTrackPlaying_ReturnsNoContent()
    {
        // Arrange
        var token = new SpotifyAccessToken { AccessToken = "test_token" };
        var response = new HttpResponseMessage(HttpStatusCode.NoContent);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        // Act
        var result = await SpotifyApi.GetCurrentTrackAsync(token, _httpClient);

        // Assert
        Assert.Equal(ApiResult.NoContent, result.Result);
        Assert.Null(result.Data);
    }
}

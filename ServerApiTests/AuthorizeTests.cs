using Infrastructure.API.ServerApi;
using Infrastructure.API.ServerApi.Models.Authorize;

namespace ServerApiTests;

public class AuthorizeTests
{
    [Test]
    public async Task BadUserAccessTokenTest()
    {
        var badUserAccessToken = new SpotifyAccessTokenDto()
        {
            AccessToken = "",
            TokenType = "",
            ExpiresDate = DateTime.UtcNow,
            RefreshToken = ""
        };
        var response = await ServerApi.AuthorizeSpotifyAsync(badUserAccessToken);
        Assert.That(response.Result, Is.EqualTo(ApiResult.UnhandledError));
        Assert.Null(response.Data);
    }
}
using Infrastructure.API;
using Infrastructure.API.SpotifyAPI;
using dotenv;
using dotenv.net;

namespace ApiTests;

public class Tests
{
    private readonly AccessTokenHandler accessTokenHandler = new AccessTokenHandler();

    [Fact]
    public async Task TestSearch()
    {
        var synclyAccessToken = await SpotifyApi.GetAppAccessToken();
        var response = await SpotifyApi
            .SearchFor()
            .AddAccessToken(synclyAccessToken)
            .AddQuestion("Maybe man")
            .SetType(QuestionType.Track)
            .SetLimit(1)
            .AddFilter(QuestionFilter.Artist, "AJR")
            .SendRequest();
        Assert.Equal("7fhiGdj0nn0ZCmIAocG8G0", response!.Data!.Tracks!.Items![0].Id!);
    }

    [Fact]
    public async Task TestGetUserInfo()
    {
        var userAccessToken = accessTokenHandler.GetAccessToken();
        var userProfile = await SpotifyApi.GetUserProfileAsync(userAccessToken);
        if (userProfile.Result is not ApiResult.Success)
            Assert.Fail("Error receiving user data");
        Assert.Equal("MrB0NAN", userProfile.Data!.DisplayName);
    }
}
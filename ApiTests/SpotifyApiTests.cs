using Infrastructure.API;
using Infrastructure.API.SpotifyAPI;

namespace ApiTests;

public class Tests
{
    [Test]
    public async Task TestSearch()
    {
        var synclyAccessToken = await SpotifyApi.GetAppAccessToken();
        var response = await SpotifyApi
            .SearchFor()
            .AddQuestion("The maybe man")
            .AddAccessToken(synclyAccessToken)
            .SetType(QuestionType.Track)
            .SetLimit(1)
            .AddFilter(QuestionFilter.Artist, "AJR")
            .SendRequest();
        Assert.That(response!.Data!.Tracks!.Items![0].Id!, Is.EqualTo("7fhiGdj0nn0ZCmIAocG8G0"));
    }
}
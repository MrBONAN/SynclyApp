using RestSharp;
using Infrastructure.API.SpotifyAPI.Models;

namespace Infrastructure.API.SpotifyAPI;

public static partial class SpotifyApi
{
    private static RestClient UserProfileClient = new RestClient("https://api.spotify.com/v1/me/");

    public static async Task<SpotifyApiResult<UserProfile>> GetUserProfileAsync(string accessToken)
    {
        var request = new RestRequest();
        request.AddHeader("Authorization", $"Bearer {accessToken}");

        var response = await UserProfileClient.ExecuteGetAsync<UserProfile>(request);

        if (response.IsSuccessful && response.Data != null)
            return new SpotifyApiResult<UserProfile>(ApiResult.Success, response.Data, response);

        return new SpotifyApiResult<UserProfile>(ApiResult.Error, null, response);
    }
}
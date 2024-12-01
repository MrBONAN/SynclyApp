using RestSharp;
using Infrastructure.API.SpotifyAPI.Models;

namespace Infrastructure.API.SpotifyAPI;

public static partial class SpotifyApi
{
    private static RestClient TopItemsClient = new RestClient("https://api.spotify.com/v1/me/");

    public static async Task<UserProfile?> GetUserProfileAsync(string accessToken)
    {
        var request = new RestRequest();
        request.AddHeader("Authorization", $"Bearer {accessToken}");

        var response = await TopItemsClient.ExecuteGetAsync<UserProfile>(request);

        if (response.IsSuccessful && response.Data != null)
            return response.Data;

        return null;
    }
}
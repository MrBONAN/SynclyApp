using RestSharp;
using Infrastructure.API.SpotifyAPI.Models;

namespace Infrastructure.API.SpotifyAPI;

public static partial class SpotifyApi
{
    private static RestClient UserDataClient = new RestClient("https://api.spotify.com/v1/me/top/");

    public static async Task<List<T>?> GetUserTopItemsAsync<T>(string accessToken, int limit = 10) where T : class
    {
        var type = typeof(T) == typeof(Track) ? "tracks" : "artists";
        var request = new RestRequest(type);
        request.AddHeader("Authorization", $"Bearer {accessToken}");
        request.AddQueryParameter("limit", limit.ToString());

        var response = await UserDataClient.ExecuteGetAsync<Paging<T>>(request);

        if (response.IsSuccessful && response.Data != null)
            return response.Data.Items;

        return null;
    }
}
using Infrastructure.API.SpotifyAPI.Models;
using RestSharp;

namespace Infrastructure.API.SpotifyAPI;

public static partial class SpotifyApi
{
    public static async Task<SpotifyApiResult<T>> GetEntityByIdAsync<T>(string accessToken, string id) where T : class
    {
        var request = new RestRequest($"/{id}");
        request.AddHeader("Authorization", $"Bearer {accessToken}");

        var response = await UserProfileClient.ExecuteGetAsync<T>(request);

        if (response.IsSuccessful && response.Data != null)
            return new SpotifyApiResult<T>(ApiResult.Success, response.Data, response);

        return new SpotifyApiResult<T>(ApiResult.Error, null, response);
    }

    public static async Task<SpotifyApiResult<List<T>, TracksAndArtistsList<T>>?> GetSeveralEntitiesById<T>(
        string accessToken,
        IEnumerable<string> ids) where T : class
    {
        var type = typeof(T) == typeof(Track) ? "tracks" : "artists";
        var request = new RestRequest($"{type}?ids={string.Join(',', ids)}");
        request.AddHeader("Authorization", $"Bearer {accessToken}");

        var response = await SpotifyClient.ExecuteGetAsync<TracksAndArtistsList<T>>(request);

        if (response.IsSuccessful && response.Data != null)
            return new SpotifyApiResult<List<T>, TracksAndArtistsList<T>>(ApiResult.Success,
                response.Data.Tracks ?? response.Data.Artists, response);

        return new SpotifyApiResult<List<T>, TracksAndArtistsList<T>>(ApiResult.Error, null, response);
    }
}
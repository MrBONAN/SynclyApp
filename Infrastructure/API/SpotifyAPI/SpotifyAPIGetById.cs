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
}
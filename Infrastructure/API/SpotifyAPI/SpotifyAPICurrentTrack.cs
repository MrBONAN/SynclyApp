using Infrastructure.API.SpotifyAPI.Models;
using RestSharp;


namespace Infrastructure.API.SpotifyAPI;

public static partial class SpotifyApi
{
    public static async Task<SpotifyApiResult<Track, PlaybackState>> GetCurrentTrackAsync(string accessToken)
    {
        var request = new RestRequest("/player");
        request.AddHeader("Authorization", $"Bearer {accessToken}");

        var response = await UserProfileClient.ExecuteGetAsync<PlaybackState>(request);

        if (response.IsSuccessful && response.Data != null && response.Data.CurrentlyPlayingType == "track")
        {
            return new SpotifyApiResult<Track, PlaybackState>(ApiResult.Success, response.Data.Item!, response);
        }
        
        return new SpotifyApiResult<Track, PlaybackState>(ApiResult.Error, null, response);
    }
}
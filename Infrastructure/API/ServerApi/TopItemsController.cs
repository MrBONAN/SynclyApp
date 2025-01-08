using Infrastructure.API.ServerApi.Models;
using Infrastructure.API.ServerApi.Models.Track;
using RestSharp;

namespace Infrastructure.API.ServerApi;

public static partial class ServerApi
{
    public static async Task<ServerApiResult<List<TrackDto>>> GetTopTracks(int userId)
    {
        var request = new RestRequest($"/api/top/track/{userId}");
        var response = await ServerClient.ExecuteGetAsync<List<TrackDto>>(request);
        if (response is { IsSuccessful: true, Data: not null })
            return new ServerApiResult<List<TrackDto>>(ApiResult.Ok, response.Data);
        return HandleError(response);
    }
    
    public static async Task<ServerApiResult<List<ArtistDto>>> GetTopArtists(int userId)
    {
        var request = new RestRequest($"/api/top/track/{userId}");
        var response = await ServerClient.ExecuteGetAsync<List<ArtistDto>>(request);
        if (response is { IsSuccessful: true, Data: not null })
            return new ServerApiResult<List<ArtistDto>>(ApiResult.Ok, response.Data);
        return HandleError(response);
    }
}
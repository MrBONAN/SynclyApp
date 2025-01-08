using Infrastructure.API.ServerApi.Models.Track;
using RestSharp;

namespace Infrastructure.API.ServerApi;

public static partial class ServerApi
{
    public static async Task<ServerApiResult<TrackDto>> GetTrackAsync(int trackId)
    {
        var request = new RestRequest($"/api/track/{trackId}");
        var response = await ServerClient.ExecuteGetAsync<TrackDto>(request);
        if (response is { IsSuccessful: true, Data: not null })
            return new ServerApiResult<TrackDto>(ApiResult.Ok, response.Data);
        return HandleError(response);
    }
    
    public static async Task<ServerApiResult<TrackDto>> GetCurrentAsync(int userId)
    {
        var request = new RestRequest($"/api/track/current/{userId}");
        var response = await ServerClient.ExecuteGetAsync<TrackDto>(request);
        if (response is { IsSuccessful: true, Data: not null })
            return new ServerApiResult<TrackDto>(ApiResult.Ok, response.Data);
        return HandleError(response);
    }
    
    public static async Task<ServerApiResult<List<TrackDto>>> GetRecentlyTracks(int userId)
    {
        var request = new RestRequest($"/api/track/recently-played/{userId}");
        var response = await ServerClient.ExecuteGetAsync<List<TrackDto>>(request);
        if (response is { IsSuccessful: true, Data: not null })
            return new ServerApiResult<List<TrackDto>>(ApiResult.Ok, response.Data);
        return HandleError(response);
    }
}
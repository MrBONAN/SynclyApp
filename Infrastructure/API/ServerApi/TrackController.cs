using Infrastructure.API.ServerApi.Models.Track;
using RestSharp;

namespace Infrastructure.API.ServerApi;

public static partial class ServerApi
{
    public static async Task<ServerApiResult<TrackDto>> GetTrack(int trackId)
    {
        var request = new RestRequest($"/api/track/{trackId}");
        var response = await ServerClient.ExecuteGetAsync<TrackDto>(request);
        if (response is { IsSuccessful: true, Data: not null })
            return new ServerApiResult<TrackDto>(ApiResult.Ok, response.Data);
        return HandleError(response);
    }
}
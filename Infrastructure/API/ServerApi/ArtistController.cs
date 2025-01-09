using Infrastructure.API.ServerApi.Models;
using RestSharp;

namespace Infrastructure.API.ServerApi;

public static partial class ServerApi
{
    public static async Task<ServerApiResult<ArtistDto>> GetArtistAsync(int artistId)
    {
        var request = new RestRequest($"/api/track/{artistId}");
        var response = await ServerClient.ExecuteGetAsync<ArtistDto>(request);
        if (response is { IsSuccessful: true, Data: not null })
            return new ServerApiResult<ArtistDto>(ApiResult.Ok, response.Data);
        return HandleError(response);
    }
}
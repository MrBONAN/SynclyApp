using Infrastructure.API.ServerApi.Models.Authorize;
using Infrastructure.API.ServerApi.Models.User;
using RestSharp;

namespace Infrastructure.API.ServerApi;

public static partial class ServerApi
{
    public static async Task<ServerApiResult<UserDto>> AuthorizeSpotifyAsync(SpotifyAccessTokenDto accessTokenDto)
    {
        var request = new RestRequest("/api/authorize/spotify")
            .AddHeader("accept", "*/*")
            .AddHeader("Content-Type", "application/json-patch+json")
            .AddJsonBody(accessTokenDto);
        var response = await ServerClient.ExecutePostAsync<UserDto>(request);
        if (response is { IsSuccessful: true, Data: not null })
            return new ServerApiResult<UserDto>(ApiResult.Ok, response.Data);
        return HandleError(response);
    }
}
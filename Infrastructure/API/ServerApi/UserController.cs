using Infrastructure.API.ServerApi.Models.User;
using RestSharp;

namespace Infrastructure.API.ServerApi;

public static partial class ServerApi
{
    public static async Task<ServerApiResult<UserDto>> GetUser(int userId)
    {
        var request = new RestRequest($"/api/user/{userId}");
        var response = await ServerClient.ExecuteGetAsync<UserDto>(request);
        if (response is { IsSuccessful: true, Data: not null })
            return new ServerApiResult<UserDto>(ApiResult.Ok, response.Data);
        return HandleError(response);
    }
    
    public static async Task<ServerApiResult<List<UserDto>>> GetAllUsers()
    {
        var request = new RestRequest($"/api/user/");
        var response = await ServerClient.ExecuteGetAsync<List<UserDto>>(request);
        if (response is { IsSuccessful: true, Data: not null })
            return new ServerApiResult<List<UserDto>>(ApiResult.Ok, response.Data);
        return HandleError(response);
    }
}
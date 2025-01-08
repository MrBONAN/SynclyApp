using Infrastructure.API.ServerApi.Models.Location;
using RestSharp;

namespace Infrastructure.API.ServerApi;

public static partial class ServerApi
{
    public static async Task<ServerApiResult<LocationDto>> UpdateLocationAsync(int userId, UpdateLocationDto location)
    {
        var request = new RestRequest($"api/location/{userId}");
        var response = await ServerClient.ExecutePostAsync<LocationDto>(request);
        if (response is { IsSuccessful: true, Data: not null })
            return new ServerApiResult<LocationDto>(ApiResult.Ok, response.Data);
        return HandleError(response);
    }
    
    public static async Task<ServerApiResult<LocationDto>> GetLocationAsync(int userId)
    {
        var request = new RestRequest($"api/location/{userId}");
        var response = await ServerClient.ExecuteGetAsync<LocationDto>(request);
        if (response is { IsSuccessful: true, Data: not null })
            return new ServerApiResult<LocationDto>(ApiResult.Ok, response.Data);
        return HandleError(response);
    }
    
    public static async Task<ServerApiResult<List<LocationDto>>> GetAllLocationsAsync()
    {
        var request = new RestRequest($"api/location");
        var response = await ServerClient.ExecuteGetAsync<List<LocationDto>>(request);
        if (response is { IsSuccessful: true, Data: not null })
            return new ServerApiResult<List<LocationDto>>(ApiResult.Ok, response.Data);
        return HandleError(response);
    }
}
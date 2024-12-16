using RestSharp;

namespace Infrastructure.API.SpotifyAPI;

public record SpotifyApiResult<T>(ApiResult Result, T? Data, RestResponse<T> Response);
public record SpotifyApiResult<T, T2>(ApiResult Result, T? Data, RestResponse<T2> Response);

public enum ApiResult
{
    Success,
    Error
}
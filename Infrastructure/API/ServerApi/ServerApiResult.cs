namespace Infrastructure.API.ServerApi;

public record ServerApiResult<T>(ApiResult Result, T? Data, string Message = "Success");

public enum ApiResult
{
    Ok,
    BadRequest,
    NotFound,
    AuthenticationError,
    SpotifyApiError,
    ServerUnreachable,
    UnhandledError
}
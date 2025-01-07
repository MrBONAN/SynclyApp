using RestSharp;

namespace Infrastructure.API.ServerApi;

public static partial class ServerApi
{
    private static ServerApiResult<T> HandleError<T>(RestResponse<T>? response) where T : class
    {
        //throw new NotImplementedException();
        return new ServerApiResult<T>(ApiResult.UnhandledError, null, "Unknown error");
    }
}
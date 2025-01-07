using RestSharp;

namespace Infrastructure.API.ServerApi;

public static partial class ServerApi
{
    private const string ServerAddress = "https://192.168.1.57:7199";
    private static RestClient ServerClient = new RestClient(ServerAddress);
}

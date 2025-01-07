using RestSharp;

namespace Infrastructure.API.ServerApi;

public static partial class ServerApi
{
    private const string ServerAddress = "http://localhost:5160";
    private static RestClient ServerClient = new RestClient(ServerAddress);
}

using RestSharp;

namespace Infrastructure.API.ServerApi;

public static partial class ServerApi
{
    private const string ServerAddress = "https://94.228.164.4:7199";
    public static RestClient ServerClient = new (ServerAddress);
}
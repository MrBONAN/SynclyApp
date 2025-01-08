using RestSharp;

namespace Infrastructure.API.ServerApi;

public static partial class ServerApi
{
    private const string ServerNgrokAddress = "https://a926-2a12-5940-a4e1-00-2.ngrok-free.app";
    public static RestClient ServerClient = new (ServerNgrokAddress);
}
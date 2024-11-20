using RestSharp;
using dotenv.net;
using Infrastructure.API.SpotifyAPI.QuestionCreator;

namespace Infrastructure.API.SpotifyAPI;

public static class SpotifyApi
{
    private static string ClientId { get; set; }
    private static string ClientSecret { get; set; }

    static SpotifyApi()
    {
        LoadEnvFiles();
    }

    private static void LoadEnvFiles()
    {
        DotEnv.Fluent().WithExceptions().WithEnvFiles("../../../../Infrastructure/API/SpotifyAPI/.env").Load();
        ClientId = Environment.GetEnvironmentVariable("CLIENT_ID") ??
                   throw new FileLoadException("App client id was not found");
        ClientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET") ??
                       throw new FileLoadException("App client secret was not found");
    }

    public static async Task<AccessToken?> GetAppAccessToken()
    {
        var client = new RestClient("https://accounts.spotify.com/api/token");
        var request = new RestRequest()
            .AddHeader("Content-Type", "application/x-www-form-urlencoded")
            .AddParameter("grant_type", "client_credentials")
            .AddParameter("client_id", ClientId)
            .AddParameter("client_secret", ClientSecret);

        var accessToken = (await client.ExecutePostAsync<AccessToken>(request)).Data;
        Console.WriteLine($"Response: {accessToken?.TokenValue}");
        return accessToken;
    }

    public static IAddQuestionStage SearchFor()
    {
        return new QuestionCreator.QuestionCreator();
    }
}
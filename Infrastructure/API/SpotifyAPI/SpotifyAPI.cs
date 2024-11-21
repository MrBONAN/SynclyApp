using System.Diagnostics;
using RestSharp;
using dotenv.net;
using Infrastructure.API.SpotifyAPI.QuestionCreator;

namespace Infrastructure.API.SpotifyAPI;

public static class SpotifyApi
{
    public static string ClientId { get; private set; }
    private static string ClientSecret { get; set; }

    static SpotifyApi()
    {
        LoadEnvFiles();
    }

    private static void LoadEnvFiles()
    {
        // DotEnv.Fluent().WithExceptions().WithEnvFiles(".env").Load();
        // ClientId = Environment.GetEnvironmentVariable("CLIENT_ID") ??
        //            throw new FileLoadException("App client id was not found");
        // ClientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET") ??
        //                throw new FileLoadException("App client secret was not found");
        ClientId = "90f13d35881a49c4b1d1f7c4ba4f040c";
        ClientSecret = "2056153472184920bee57c72cb4d9f0d";
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
        return accessToken;
    }

    public static IAddQuestionStage SearchFor()
    {
        return new QuestionCreator.QuestionCreator();
    }
}
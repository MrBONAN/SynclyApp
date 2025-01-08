using App.UserAuthorization.SpotifyAuthorization;
using App.UserAuthorization.SpotifyAuthorization.Models;
using Infrastructure.API.SpotifyAPI;

namespace App;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; }
    public App(ISpotifyAuthManager spotifyAuthManager, ISpotifyAccessTokenService spotifyAccessToken, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        Services = serviceProvider;
        var token = Task.Run(() => spotifyAccessToken.GetAsync()).Result;
        if (token.Result != AccessTokenResult.Success)
            MainPage = Services.GetRequiredService<SignIn>();
        else
            MainPage = Services.GetRequiredService<Map>();
    }
    
    protected override async void OnStart()
    {
        var restClient = await CertificateHandler.CreateCustomHttpClientHandler();
        ServerApi.ServerClient = restClient;
    }
}
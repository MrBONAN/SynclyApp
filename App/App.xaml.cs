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
            MainPage = Services.GetRequiredService<MainPage>();
        else
            MainPage = Services.GetRequiredService<Map>();
    }
}
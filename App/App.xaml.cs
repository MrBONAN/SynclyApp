using App.UserAuthorization.SpotifyAuthorization;
using App.UserAuthorization.SpotifyAuthorization.Models;
using Infrastructure.API.SpotifyAPI;

namespace App;

public partial class App : Application
{
    public App(ISpotifyAccessTokenService spotifyAccessToken)
    {
        InitializeComponent();
        var token = Task.Run(() => spotifyAccessToken.GetAsync()).Result;
        if (token.Result != AccessTokenResult.Success)
            MainPage = new Map(spotifyAccessToken);
        else
            MainPage = new Map(spotifyAccessToken);
    }
}
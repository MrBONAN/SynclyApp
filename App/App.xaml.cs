using App.UserAuthorization.SpotifyAuthorization.Models;
using Infrastructure.API.SpotifyAPI;

namespace App;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new Map();
        // var token = Task.Run(() => SpotifyAccessToken.Get()).Result;
        //  if (token.Result != AccessTokenResult.Success)
        //      MainPage = new MainPage(); 
        //  else
        //      MainPage = new Map();
    }
}
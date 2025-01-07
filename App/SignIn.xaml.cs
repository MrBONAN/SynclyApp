using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.UserAuthorization.SpotifyAuthorization;
using App.UserAuthorization.SpotifyAuthorization.Models;

namespace App;

public partial class SignIn : ContentPage
{
    private ISpotifyAuthManager spotifyAuthManager;
    private ISpotifyAccessTokenService spotifyAccessToken;

    public SignIn()
    {
        InitializeComponent();
        //AnimateBackground();
        spotifyAuthManager = App.Services.GetRequiredService<ISpotifyAuthManager>();
        spotifyAccessToken = App.Services.GetRequiredService<ISpotifyAccessTokenService>();
    }

    private async void OnSpotifyAuthButtonClicked(object sender, EventArgs e)
    {
        var logInResult = await spotifyAuthManager.LogInAsync();
        if (logInResult == LogInResult.Success)
        {
            Application.Current.MainPage = new Map(spotifyAccessToken);
        }

        await Application.Current?.MainPage?.DisplayAlert("Результат входа", logInResult.ToString(), "ОК")!;
    }
}
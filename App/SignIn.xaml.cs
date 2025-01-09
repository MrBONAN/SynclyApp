using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.UserAuthorization;
using App.UserAuthorization.SpotifyAuthorization;
using App.UserAuthorization.SpotifyAuthorization.Models;

namespace App;

public partial class SignIn : ContentPage
{
    private ISpotifyAuthManager spotifyAuthManager;
    private IUserDataHandler userDataHandler;

    public SignIn()
    {
        InitializeComponent();
        spotifyAuthManager = App.Services.GetRequiredService<ISpotifyAuthManager>();
        userDataHandler = App.Services.GetRequiredService<IUserDataHandler>();
    }

    private async void OnSpotifyAuthButtonClicked(object sender, EventArgs e)
    {
        var logInResult = await spotifyAuthManager.LogInAsync();
        if (logInResult == LogInResult.Success)
            Application.Current.MainPage = new Map();

        await Application.Current?.MainPage?.DisplayAlert("Результат входа", logInResult.ToString(), "ОК")!;
    }
}
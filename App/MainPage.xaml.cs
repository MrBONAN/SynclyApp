using System.Diagnostics;
using App.UserAuthentication.SpotifyAuthentication;
using Infrastructure;

namespace App;

public partial class MainPage : ContentPage
{
    int count = 0;

    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnCounterClicked(object sender, EventArgs e)
    {
        await Authenticate();
    }

    private async Task Authenticate()
    {
        var authPkceResponse = await SpotifyPkceAuthentication.AuthenticateWithPkceAsync();
        if (authPkceResponse.Result is not AuthenticationResult.Success)
            return;
        var accessToken = await SpotifyPkceAuthentication.ExchangeCodeForPkceTokenAsync(authPkceResponse.Code!,
            authPkceResponse.CodeVerifier!);
    }
}
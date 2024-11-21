using System.Diagnostics;
using App.UserAuthentication.SpotifyAuthentication;
using Infrastructure.API;
using Infrastructure.API.SpotifyAPI;

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
        var response = await SpotifyApi
            .SearchFor()
            .AddQuestion("The maybe man")
            .AddAccessToken(accessToken.AccessToken!)
            .SetType(QuestionType.Track)
            .SetLimit(1)
            .AddFilter(QuestionFilter.Artist, "AJR")
            .SendRequest();
    }
}
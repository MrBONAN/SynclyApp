using System.Diagnostics;
using App.UserAuthorization.SpotifyAuthorization;
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
        var authPkceResponse = await SpotifyPkceAuthorization.AuthorizeWithPkceAsync();
        if (authPkceResponse.Result is not AuthorizationResult.Success)
            return;
        var accessToken = await SpotifyPkceAuthorization.ExchangeCodeForPkceTokenAsync(authPkceResponse.Code!,
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

    private async void SaveCurrentTime(object sender, EventArgs e)
    {
        await SecureStorage.Default.SetAsync("current_time", DateTime.Now.ToString("F"));
    }

    private async void DisplaySavedTime(object sender, EventArgs e)
    {
        var time = await SecureStorage.Default.GetAsync("current_time") ?? "null";
        await Application.Current?.MainPage?.DisplayAlert("Сохранённое время", time, "ОК")!;
    }
}
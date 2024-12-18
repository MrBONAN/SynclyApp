using App.UserAuthorization.SpotifyAuthorization;
using System.Diagnostics;
using System.Net;
using App.UserAuthorization;
using App.UserAuthorization.SpotifyAuthorization.Models;
using App.UserAuthorization.YandexAuthorization;
using Infrastructure.API.SpotifyAPI;
using Infrastructure.API;
using Infrastructure.API.SpotifyAPI.Models;

namespace App;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void SpotifyAuthenticate(object sender, EventArgs e)
    {
        var logInResult = await SpotifyAuthManager.LogIn();
        await Application.Current?.MainPage?.DisplayAlert("Результат входа через Spotify", logInResult.ToString(), "ОК")!;
    }

    private async void FindTrack(object sender, EventArgs e)
    {
        var question = await Application.Current.MainPage.DisplayPromptAsync(
            "Поиск трека",
            "Пожалуйста, введите название трека:",
            "OK",
            "Отмена",
            "Трек",
            50,
            Keyboard.Text,
            null);

        if (string.IsNullOrEmpty(question))
        {
            Debug.WriteLine("Вы не ввели текст");
            return;
        }

        Debug.WriteLine($"Вы ввели: {question}");
        var token = await SpotifyAccessToken.Get();
        if (token.Result is not AccessTokenResult.Success)
        {
            await Application.Current.MainPage?.DisplayAlert("Ошибка при чтении токена",
                token.Result.ToString(), "ОК")!;
            return;
        }

        var response = await SpotifyApi.SearchFor()
            .AddAccessToken(token.Value!)
            .AddQuestion(question)
            .SetType(QuestionType.Track)
            .SetLimit(1)
            .SendRequest();

        if (response.StatusCode is HttpStatusCode.Forbidden)
        {
            await Application.Current.MainPage?.DisplayAlert("Запрет на доступ к ресурсу",
                "Запрошенная страница не доступна в вашем регионе", "ОК")!;
            return;
        }

        var trackData = response.Data.Tracks.Items.First();
        await Application.Current.MainPage?.DisplayAlert("Результат поиска",
            $"Spotify ID: {trackData.Id}, " +
            $"url: {trackData.ExternalUrls!.Spotify!}, " +
            $"artist: {trackData.Artists!.First().Name}",
            "OK")!;
    }

    private async void GetTopTracks(object sender, EventArgs e)
    {
        var token = await SpotifyAccessToken.Get();
        var topTracks = await SpotifyApi.GetUserTopItemsAsync<Track>(token.Value!);
        if (topTracks.Result is not ApiResult.Success) return;
        await Application.Current.MainPage?.DisplayAlert("Топ треков",
            String.Join("\n", topTracks.Data!.Select((track, i) => $"{i + 1}: {track.Name}")),
            "OK")!;
    }

    private async void GetTopArtists(object sender, EventArgs e)
    {
        var token = await SpotifyAccessToken.Get();
        var topArtists = await SpotifyApi.GetUserTopItemsAsync<Artist>(token.Value!);
        if (topArtists.Result is not ApiResult.Success) return;
        await Application.Current.MainPage?.DisplayAlert("Топ артистов",
            String.Join("\n", topArtists.Data!.Select((artist, i) => $"{i + 1}: {artist.Name}")),
            "OK")!;
    }

    private async void GetSeveralTracks(object sender, EventArgs e)
    {
        var token = await SpotifyAccessToken.Get();
        var severalTracks = await SpotifyApi.GetSeveralEntitiesById<Track>(token.Value!,
            new[] { "26wLOs3ZuHJa2Ihhx6QIE6", "5flerg6aEao2VayZezVlgu", "7LHAKF7pBqHch8o6Yo0ad5"});
        if (severalTracks.Result is not ApiResult.Success) return;
        await Application.Current.MainPage?.DisplayAlert("Три запрошенных трека",
            String.Join("\n", severalTracks.Data!.Select((track, i) => $"{i + 1}: {track.Name}")),
            "OK")!;
    }
    
    private async void GetSeveralArtists(object sender, EventArgs e)
    {
        var token = await SpotifyAccessToken.Get();
        var severalArtists = await SpotifyApi.GetSeveralEntitiesById<Artist>(token.Value!,
            new[] { "6s22t5Y3prQHyaHWUN1R1C", "6DdeqvIfYu3sH02gdavOu2", "0LcJLqbBmaGUft1e9Mm8HV"});
        if (severalArtists.Result is not ApiResult.Success) return;
        await Application.Current.MainPage?.DisplayAlert("Три запрошенных артиста",
            String.Join("\n", severalArtists.Data!.Select((artist, i) => $"{i + 1}: {artist.Name}")),
            "OK")!;
    }

    private async void GetUserData(object sender, EventArgs e)
    {
        var token = await SpotifyAccessToken.Get();
        var userProfile = await SpotifyApi.GetUserProfileAsync(token.Value!);
        if (userProfile.Result is not ApiResult.Success) return;
        await Application.Current.MainPage?.DisplayAlert("Данные пользователя",
            $"Id: {userProfile.Data!.Id}, name: {userProfile.Data.DisplayName}",
            "OK")!;
    }

    private async void GetCurrentTrack(object sender, EventArgs e)
    {
        var token = await SpotifyAccessToken.Get();
        var currentTrack = await SpotifyApi.GetCurrentTrackAsync(token.Value!);
        if (currentTrack.Result is not ApiResult.Success) return;
        await Application.Current.MainPage?.DisplayAlert("Текущий трек",
            $"Name: {currentTrack.Data!.Name}",
            "OK")!;
    }

    private void LogOut(object sender, EventArgs e)
    {
        SpotifyAuthManager.LogOut();
    }

    private async void YandexAuthenticate(object sender, EventArgs e)
    {
        var logInResult = await CreateYandexAuthPage();
        await Application.Current?.MainPage?.DisplayAlert("Результат входа через яндекс", logInResult.ToString(),
            "ОК")!;
        if (logInResult is LogInResult.Success)
        {
            var accessToken = (await YandexAccessToken.Get()).Value;
            await Application.Current.MainPage?.DisplayAlert("Полученный токен: ", accessToken, "ОК")!;
        }
    }

    private async Task<LogInResult> CreateYandexAuthPage()
    {
        var tokenResultTask = new TaskCompletionSource<LogInResult>();
        var browserPage = new YandexAuthorizationPage(tokenResultTask);
        await Navigation.PushModalAsync(browserPage);
        return await tokenResultTask.Task;
    }
}
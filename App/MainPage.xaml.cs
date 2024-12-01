using App.UserAuthorization.SpotifyAuthorization;
using System.Diagnostics;
using System.Net;
using App.UserAuthorization.SpotifyAuthorization.Models;
using Infrastructure.API;
using Infrastructure.API.SpotifyAPI;

namespace App;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void Authenticate(object sender, EventArgs e)
    {
        var logInResult = await SpotifyAuthManager.LogIn();
        await Application.Current?.MainPage?.DisplayAlert("Результат входа", logInResult.ToString(), "ОК")!;
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
using App.UserAuthorization.SpotifyAuthorization;
using App.UserAuthorization;
using Infrastructure.API.ServerApi;

namespace App;

public partial class MainPage : ContentPage
{
    private readonly ISpotifyAuthManager spotifyAuthManager;
    private readonly IUserDataHandler userDataHandler;

    public MainPage(ISpotifyAuthManager spotifyAuthManager, IUserDataHandler userDataHandler)
    {
        this.spotifyAuthManager = spotifyAuthManager;
        this.userDataHandler = userDataHandler;
        InitializeComponent();
    }

    private async void Authenticate(object sender, EventArgs e)
    {
        var logInResult = await spotifyAuthManager.LogInAsync();
        await Application.Current?.MainPage?.DisplayAlert("Результат входа", logInResult.ToString(), "ОК")!;
    }

    // private async void FindTrack(object sender, EventArgs e)
    // {
    //     var question = await Application.Current.MainPage.DisplayPromptAsync(
    //         "Поиск трека",
    //         "Пожалуйста, введите название трека:",
    //         "OK",
    //         "Отмена",
    //         "Трек",
    //         50,
    //         Keyboard.Text,
    //         null);
    //
    //     if (string.IsNullOrEmpty(question))
    //     {
    //         Debug.WriteLine("Вы не ввели текст");
    //         return;
    //     }
    //
    //     Debug.WriteLine($"Вы ввели: {question}");
    //     var token = await spotifyAccessToken.GetAsync();
    //     if (token.Result is not AccessTokenResult.Success)
    //     {
    //         await Application.Current.MainPage?.DisplayAlert("Ошибка при чтении токена",
    //             token.Result.ToString(), "ОК")!;
    //         return;
    //     }
    //
    //     var response = await SpotifyApi.SearchFor()
    //         .AddAccessToken(token.Value!)
    //         .AddQuestion(question)
    //         .SetType(QuestionType.Track)
    //         .SetLimit(1)
    //         .SendRequest();
    //
    //     if (response.StatusCode is HttpStatusCode.Forbidden)
    //     {
    //         await Application.Current.MainPage?.DisplayAlert("Запрет на доступ к ресурсу",
    //             "Запрошенная страница не доступна в вашем регионе", "ОК")!;
    //         return;
    //     }
    //
    //     var trackData = response.Data.Tracks.Items.First();
    //     await Application.Current.MainPage?.DisplayAlert("Результат поиска",
    //         $"Spotify ID: {trackData.Id}, " +
    //         $"url: {trackData.ExternalUrls!.Spotify!}, " +
    //         $"artist: {trackData.Artists!.First().Name}",
    //         "OK")!;
    // }

    private async void GetUserLocation(object sender, EventArgs e)
    {
        var userId = await userDataHandler.GetUserIdAsync();
        if (userId is null) return;
        var topTracks = await ServerApi.GetLocationAsync(userId.Value);
        if (topTracks.Result is not ApiResult.Ok) return;
        await Application.Current!.MainPage?.DisplayAlert($"Локация пользователя {userId.Value}",
            $"Широта: ",
            "OK")!;
    }

    private async void GetTopTracks(object sender, EventArgs e)
    {
        var userId = await userDataHandler.GetUserIdAsync();
        if (userId is null) return;
        var topTracks = await ServerApi.GetTopTracksAsync(userId.Value);
        if (topTracks.Result is not ApiResult.Ok) return;
        await Application.Current!.MainPage?.DisplayAlert("Топ треков",
            String.Join("\n", topTracks.Data!.Select((track, i) => $"{i + 1}: {track.Name}")),
            "OK")!;
    }

    private async void GetTopArtists(object sender, EventArgs e)
    {
        var userId = await userDataHandler.GetUserIdAsync();
        if (userId is null) return;
        var topArtists = await ServerApi.GetTopArtistsAsync(userId.Value);
        if (topArtists.Result is not ApiResult.Ok) return;
        await Application.Current!.MainPage?.DisplayAlert("Топ артистов",
            String.Join("\n", topArtists.Data!.Select((artist, i) => $"{i + 1}: {artist.Name}")),
            "OK")!;
    }
}

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

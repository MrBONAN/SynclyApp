using System.Collections.ObjectModel;
using App.UserAuthorization.SpotifyAuthorization;
using App.UserAuthorization.SpotifyAuthorization.Models;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Core.Extensions;
using The49.Maui.BottomSheet;
using CommunityToolkit.Maui.Views;
using Domain;
using Infrastructure.API.SpotifyAPI;

namespace ProfileBottomSheet;

public partial class Sheet : BottomSheet
{
    public Sheet()
    {
        InitializeComponent();
        BindingContext = new ProfileBottomSheetViewModel();
    }
}

public class ProfileBottomSheetViewModel
{
    public ObservableCollection<Track> Tracks { get; set; }
    public ObservableCollection<Artist> Artists { get; set; }

    public ProfileBottomSheetViewModel()
    {
        var accessToken = SpotifyAccessToken.Get().Result;
        if (accessToken.Result != AccessTokenResult.Success)
        {
            throw new InvalidOperationException("Unable to get access token");
        }

        var result = Task.Run(() => SpotifyApi.GetUserTopItemsAsync<Track>(accessToken.Value)).Result;
        if (result.Result is not ApiResult.Success)
        {
            Application.Current?.MainPage?.DisplayAlert("Пиздец", $"Ну что за хуйня\n{result.Response.Content}", "ОК");
        }

        Artists = Task.Run(() => SpotifyApi
                .GetUserTopItemsAsync<Artist>(accessToken.Value)
            ).Result
            .Data
            .ToObservableCollection();
    }

    private async void GetTopTracks<T>()
        where T : Track
    {
        var token = await SpotifyAccessToken.Get();
        var topTracks = await SpotifyApi.GetUserTopItemsAsync<T>(token.Value!);
        if (!(topTracks?.Result is ApiResult.Success)) return;
        await Application.Current.MainPage?.DisplayAlert("Топ треков",
            String.Join("\n", topTracks.Data!.Select((track, i) => $"{i + 1}: {track.Name}")),
            "OK")!;
    }
}
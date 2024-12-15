using System.Collections.ObjectModel;
using App.UserAuthorization.SpotifyAuthorization;
using App.UserAuthorization.SpotifyAuthorization.Models;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Core.Extensions;
using The49.Maui.BottomSheet;
using CommunityToolkit.Maui.Views;
using Infrastructure.API.SpotifyAPI;
using Infrastructure.API.SpotifyAPI.Models;

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
    public ObservableCollection<Track>? Tracks { get; set; }
    public ObservableCollection<Artist>? Artists { get; set; }

    public ProfileBottomSheetViewModel()
    {
        Tracks = Task.Run(GetTop<Track>).Result;
        Artists = Task.Run(GetTop<Artist>).Result;
    }

    private async Task<ObservableCollection<T>?> GetTop<T>()
        where T : class
    {
        var token = await SpotifyAccessToken.Get();
        var top = await SpotifyApi.GetUserTopItemsAsync<T>(token.Value!);
        return top?.Result is not ApiResult.Success ? null : top.Data.ToObservableCollection();
    }
}
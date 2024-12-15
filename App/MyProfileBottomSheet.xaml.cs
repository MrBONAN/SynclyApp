using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using App.UserAuthorization.SpotifyAuthorization;
using App.UserAuthorization.SpotifyAuthorization.Models;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Core.Extensions;
using The49.Maui.BottomSheet;
using CommunityToolkit.Maui.Views;
using Domain;
using Infrastructure.API.SpotifyAPI;
using AccessToken = App.UserAuthorization.SpotifyAuthorization.Models.AccessToken;
using Artist = Infrastructure.API.SpotifyAPI.Models.Artist;
using Track = Infrastructure.API.SpotifyAPI.Models.Track;

namespace ProfileBottomSheet;

public partial class Sheet : BottomSheet
{
    public Sheet()
    {
        InitializeComponent();
        BindingContext = new ProfileBottomSheetViewModel();
        InitializeData();
        
    }

    private async void InitializeData()
    {
        if (BindingContext is not ProfileBottomSheetViewModel viewModel) return;
        var loadingTasks = new List<Task>
        {
            viewModel.LoadDataAsync()
        };

        // Пока данные загружаются, показываем индикатор загрузки.
        await Task.WhenAny(loadingTasks);
    }

    private async void OnGridTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is string url)
                await Launcher.Default.OpenAsync(url);
    }
}

public class ProfileBottomSheetViewModel : INotifyPropertyChanged
{
    private bool _isLoadingTracks = true;
    private bool _isLoadingArtists = true;
    
    public bool IsLoadingTracks
    {
        get => _isLoadingTracks;
        set
        {
            if (_isLoadingTracks == value) return;
            _isLoadingTracks = value;
            OnPropertyChanged();
        }
    }
    
    public bool IsLoadingArtists
    {
        get => _isLoadingArtists;
        set
        {
            if (_isLoadingArtists == value) return;
            _isLoadingArtists = value;
            OnPropertyChanged();
        }
    }
    
    public ObservableCollection<Domain.Track> Tracks { get; set; } = new();
    public ObservableCollection<Domain.Artist> Artists { get; set; } = new();

    public async Task LoadDataAsync()
    {
        var loadTracks = GetTopTracks();
        var loadArtists = GetTopArtists();

        await Task.WhenAll(loadTracks, loadArtists);
        
        Tracks = await loadTracks;
        Artists = await loadArtists;

        OnPropertyChanged(nameof(Tracks));
        IsLoadingTracks = false;

        OnPropertyChanged(nameof(Artists));
        IsLoadingArtists = false;
    }

    private async Task<ObservableCollection<Domain.Artist>> GetTopArtists()
    {
        var token = await SpotifyAccessToken.Get();
        var top = await SpotifyApi.GetUserTopItemsAsync<Artist>(token.Value!);
    
        if (top?.Result is not ApiResult.Success || top.Data == null)
            return new ObservableCollection<Domain.Artist>();

        return top.Data
            .Where(x => x != null)
            .Select(x => new Domain.Artist(x))
            .ToObservableCollection();
    }

    private async Task<ObservableCollection<Domain.Track>> GetTopTracks()
    {
        var token = await SpotifyAccessToken.Get();
        var top = await SpotifyApi.GetUserTopItemsAsync<Track>(token.Value!);
    
        if (top?.Result is not ApiResult.Success || top.Data == null)
            return new ObservableCollection<Domain.Track>();

        return top.Data
            .Where(x => x != null)
            .Select(x => new Domain.Track(x))
            .ToObservableCollection();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Maui.Core.Extensions;
using Domain;
using The49.Maui.BottomSheet;
using Infrastructure.API.ServerApi;
using ApiResult = Infrastructure.API.ServerApi.ApiResult;

namespace ProfileBottomSheet;

public partial class Sheet : BottomSheet
{
    public Sheet(int id)
    {
        InitializeComponent();
        BindingContext = new ViewModel(id);
        var viewModel = BindingContext as ViewModel;
        SpotifyAccountButton.Clicked += viewModel.OpenSpotifyProfile;
        WriteMessageButton.Clicked += viewModel.OpenChat;
        InitializeData();
    }

    private async void InitializeData()
    {
        if (BindingContext is not ViewModel viewModel) return;
        var loadingTasks = new List<Task>
        {
            viewModel.LoadDataAsync()
        };
        await Task.WhenAny(loadingTasks);
    }
}

public class ViewModel(int id) : INotifyPropertyChanged
{
    private bool _isLoadingTracks = true;
    private bool _isLoadingArtists = true;
    private bool _isLoadingRecentlyPlayed = true;
    private User _currentUser;
    
    public readonly int Id = id;
    
    public User CurrentUser
    {
        get => _currentUser;
        set
        {
            if (_currentUser == value) return;
            _currentUser = value;
            OnPropertyChanged();
        }
    }

    public string CurrentUserName { get; set; }
    public string CurrentUserImage { get; set; }

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
    
    public bool IsLoadingRecentlyPlayed
    {
        get => _isLoadingRecentlyPlayed;
        set
        {
            if (_isLoadingRecentlyPlayed == value) return;
            _isLoadingRecentlyPlayed = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<Track> Tracks { get; set; } = new();
    public ObservableCollection<Artist> Artists { get; set; } = new();
    public ObservableCollection<Track> RecentlyPlayed { get; set; } = new();

    public async Task LoadDataAsync()
    {
        var loadTracks = GetTopTracks();
        var loadArtists = GetTopArtists();
        var loadRecentlyPlayed = GetTopRecentlyTracks();
        var resultData = (await ServerApi.GetUserAsync(Id)).Data;
        
        if (resultData != null)
        {
            CurrentUser = new User();
            await CurrentUser.Initialize(resultData);
        }

        await Task.WhenAll(loadTracks, loadArtists);
        Tracks = await loadTracks;
        Artists = await loadArtists;
        RecentlyPlayed = await loadRecentlyPlayed;

        OnPropertyChanged(nameof(Tracks));
        IsLoadingTracks = false;

        OnPropertyChanged(nameof(Artists));
        IsLoadingArtists = false;
        
        OnPropertyChanged(nameof(RecentlyPlayed));
        IsLoadingRecentlyPlayed = false;

        CurrentUserName = CurrentUser.Name;
        CurrentUserImage = CurrentUser.ProfileImageURL;
        OnPropertyChanged(nameof(CurrentUserName));
        OnPropertyChanged(nameof(CurrentUserImage));
    }

    private async Task<ObservableCollection<Artist>> GetTopArtists()
    {
        var top = await ServerApi.GetTopArtistsAsync(Id);

        if (top.Result is not ApiResult.Ok || top.Data == null)
            return new ObservableCollection<Artist>();

        return top.Data
            .Where(x => x != null)
            .Select(x => new Artist(x))
            .ToObservableCollection();
    }

    private async Task<ObservableCollection<Track>> GetTopTracks()
    {
        var top = await ServerApi.GetTopTracksAsync(Id);

        if (top?.Result is not ApiResult.Ok || top.Data == null)
            return new ObservableCollection<Track>();

        return top.Data
            .Where(x => x != null)
            .Select(x => new Track(x))
            .ToObservableCollection();
    }
    
    private async Task<ObservableCollection<Track>> GetTopRecentlyTracks()
    {
        var recentlyTracks = await ServerApi.GetRecentlyTracks(Id);

        if (recentlyTracks?.Result is not ApiResult.Ok || recentlyTracks.Data == null)
            return new ObservableCollection<Track>();

        return recentlyTracks.Data
            .Where(x => x != null)
            .Select(x => new Track(x))
            .ToObservableCollection();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public async void OpenSpotifyProfile(object? sender, EventArgs e)
    {
        var link = CurrentUser.MusicAppLinks[MusicServices.Spotify];
        if (link != null)
            await Launcher.OpenAsync(link);
    }

    public async void OpenChat(object? sender, EventArgs e)
    {
        if (CurrentUser == null) return;
        var page = new Chat.Sheet(CurrentUser.Id);
        await page.ShowAsync();
    }
}
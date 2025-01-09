using System.Windows.Input;
using Infrastructure.API.ServerApi.Models;

namespace Domain;

public class Artist
{
    public ICommand OpenArtistOnSpotify { get; set; }
    public int? Id { get; private set; }
    public string? Name { get; private set; }
    public List<string>? Genres { get; private set; }
    public string? ProfileImageURL { get; private set; }
    public Dictionary<MusicServices, string?> Links { get; private set; } = new()
    {
        { MusicServices.Spotify, null },
        { MusicServices.YandexMusic, null }
    };

    //public Artist(string? id, string? profileImageURL, string? name)
    //{
    //    Id = id;
    //    Name = name;
    //    ProfileImageURL = profileImageURL;
    //    OpenArtistOnSpotify = new Command(Open);
    //}
    
    public Artist(ArtistDto artist)
    {
        Id = artist.Id;
        Name = artist.Name;
        ProfileImageURL = artist.Links.ExternalImageLink;
        Links[MusicServices.Spotify] = artist.Links.ExternalLink;
        OpenArtistOnSpotify = new Command(Open);
    }

    //public Artist(Infrastructure.API.SpotifyAPI.Models.Artist artist)
    //{
    //    Id = artist.Id;
    //    Name = artist.Name;
    //    ProfileImageURL = artist.Images != null ? artist.Images.FirstOrDefault()?.Url : null;
    //    Links[MusicServices.Spotify] = artist.Uri;
    //    Genres = artist.Genres;
    //    OpenArtistOnSpotify = new Command(Open);
    //}

    private async void Open(object obj)
    {
        if (Links[MusicServices.Spotify] != null)
            await Launcher.OpenAsync(Links[MusicServices.Spotify]);
    }
    
    public string ToString() => Name;
}
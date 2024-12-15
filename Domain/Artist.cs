namespace Domain;

public class Artist
{
    public string? Id { get; private set; }
    public string? Name { get; private set; }
    public List<string>? Genres { get; private set; }
    public string? ProfileImageURL { get; private set; }

    public Dictionary<MusicServices, string?> Links { get; private set; } = new()
    {
        { MusicServices.Spotify, null },
        { MusicServices.YandexMusic, null }
    };

    public Artist(string? id, string? profileImageURL, string? name)
    {
        Id = id;
        Name = name;
        ProfileImageURL = profileImageURL;
    }

    public Artist(Infrastructure.API.SpotifyAPI.Models.Artist artist)
    {
        Id = artist.Id;
        Name = artist.Name;
        ProfileImageURL = artist.Images.FirstOrDefault()?.Url;
        Links[MusicServices.Spotify] = artist.Uri;
        Genres = artist.Genres;
    }

    public string ToString() => Name;
}
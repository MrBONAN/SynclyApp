namespace Domain;

public class Artist
{
    public readonly string? Id;
    public readonly string? Name;
    public readonly List<string>? Genres;
    public readonly string? ProfileImageURL;
    public readonly Dictionary<MusicServices, string?> Links = new()
    {
        {MusicServices.Spotify, null},
        {MusicServices.YandexMusic, null}
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
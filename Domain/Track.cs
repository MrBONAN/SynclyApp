namespace Domain;

public class Track
{
    public readonly string Id;
    public readonly string Name;
    public readonly IEnumerable<Artist> Artists;
    public readonly Dictionary<MusicServices, string?> Links = new()
    {
        {MusicServices.Spotify, null},
        {MusicServices.YandexMusic, null}
    };
    public readonly string CoverUrl;

    public Track(string id, string name, List<Artist> artistId, string linkOnPlatform, string coverUrl)
    {
        Id = id;
        Name = name;
        Artists = artistId;
        Links[MusicServices.Spotify] = linkOnPlatform;
        CoverUrl = coverUrl;
    }
    
    public Track(Infrastructure.API.SpotifyAPI.Models.Track track)
    {
        Id = track.Id!;
        Name = track.Name!;
        Artists = track.Artists.Select(artist => new Artist(artist));
        Links[MusicServices.Spotify] = track.Uri;
        CoverUrl = track.Images.FirstOrDefault()?.Url;
    }
}
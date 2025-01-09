using Infrastructure.API.ServerApi;
using Infrastructure.API.ServerApi.Models.User;

namespace Domain;

public class User
{
    public string Name { get; set; }
    public Dictionary<MusicServices, string?> MusicAppLinks { get; set; } = new()
    {
        {MusicServices.Spotify, null},
        {MusicServices.YandexMusic, null}
    };
    public int Id { get; private set; }
    public string ProfileImageURL { get; set; }
    public Location Location { get; set; }
    public IEnumerable<Track> TopTracks { get; set; }
    public IEnumerable<Artist> TopArtists { get; set; }

    public User(int id)
    {
        Id = id;
        Name = $"User {id}";
    }
    
    public User()
    {
        
    }

    public async Task Initialize(UserDto user)
    {
        Id = user.Id;
        Name = user.Name;
        MusicAppLinks[MusicServices.Spotify] = user.Links.ExternalLink;
        var locationDto = (await ServerApi.GetLocationAsync(Id)).Data;
        Location = new Location()
        {
            Latitude = (double)locationDto.Latitude,
            Longitude = (double)locationDto.Longitude
        };
        TopTracks = (await ServerApi.GetTopTracksAsync(Id)).Data
            .Select(x => new Track(x));
        TopArtists = (await ServerApi.GetTopArtistsAsync(Id)).Data
            .Select(x => new Artist(x));
        ProfileImageURL = user.Links.ExternalImageLink;
    }
}
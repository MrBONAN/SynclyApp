using System.Windows.Input;
using Infrastructure.API.ServerApi.Models.Track;

namespace Domain;

public class Track
{
    public ICommand OpenTrackOnSpotify { get; }
    public int Id { get; private set; }
    public  string Name { get; private set; }
    public  IEnumerable<Artist> Artists { get; private set; }

    public string ArtistsName => String.Join(", ", Artists.Select(a => a.Name));
    public  Dictionary<MusicServices, string?> Links  { get; private set; } = new()
    {
        {MusicServices.Spotify, null},
        {MusicServices.YandexMusic, null}
    };
    public  string CoverUrl { get; private set; }

    //public Track(string id, string name, List<Artist> artistId, string linkOnPlatform, string coverUrl)
    //{
    //    Id = id;
    //    Name = name;
    //    Artists = artistId;
    //    Links[MusicServices.Spotify] = linkOnPlatform;
    //    CoverUrl = coverUrl;
    //    OpenTrackOnSpotify = new Command(Open);
    //}

   // public Track(Infrastructure.API.SpotifyAPI.Models.Track track)
   // {
   //     Id = track.Id!;
   //     Name = track.Name!;
   //     Artists = track.Artists?.Where(x => x != null).Select(artist => new Artist(artist)).ToList();
   //     Links[MusicServices.Spotify] = track.Uri;
   //     CoverUrl = track.Images.FirstOrDefault()?.Url;
   //     OpenTrackOnSpotify = new Command(Open);
   // }
    
    public Track(TrackDto track)
    {
        Id = track.Id;
        Name = track.Name;
        Artists = track.Artists.Select(x => new Artist(x)).ToList();
        Links[MusicServices.Spotify] = track.Links.ExternalLink;
        CoverUrl = track.Links.ExternalImageLink;
        OpenTrackOnSpotify = new Command(Open);
    }
    
    private async void Open(object obj)
    {
        if (Links[MusicServices.Spotify] != null)
            await Launcher.OpenAsync(Links[MusicServices.Spotify]);
    }
}
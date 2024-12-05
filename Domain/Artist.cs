namespace Domain;

public class Artist
{
    public readonly string Id;
    public IEnumerable<Track> Tracks { get; set; }
    public readonly string PhotoPath;

    public Artist(string id, string photoPath, IEnumerable<Track> tracks)
    {
        Id = id;
        PhotoPath = photoPath;
        Tracks = tracks;
    }

    public void UpdateTracks(IEnumerable<Track> tracks)
    {
        Tracks = Tracks.Concat(tracks);
    }
}
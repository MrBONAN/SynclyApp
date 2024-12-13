namespace Domain;

public class User
{
    public string Name {get; set;}
    public Dictionary<string, string> MusicAppLinks { get; set; }
    public readonly string Id;
    public Location Location { get; set; }
    public IEnumerable<Track> TopTracks { get; set; }
    public IEnumerable<Artist> TopArtists { get; set; }

    public User(string id, string name)
    {
        Id = id;
        Name = name;
    }

    public void AddApp(string app, string musicAppLink)
    {
        MusicAppLinks[app] = musicAppLink;
    }
}
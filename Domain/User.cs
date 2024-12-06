namespace Domain;

public class User
{
    public string Name {get; set;}
    public Dictionary<string, string> MusicAppLinks { get; set; }
    public readonly string Id;
    public MapLocation Location { get; set; }
    public IEnumerable<Track> TopTenTracks { get; set; }
    public IEnumerable<Artist> TopTenArtists { get; set; }

    public User(string name, string id, Dictionary<string, string> musicAppLinks)
    {
        Name = name;
        Id = id;
        MusicAppLinks = musicAppLinks;
    }

    public void AddApp(string app, string musicAppLink)
    {
        MusicAppLinks[app] = musicAppLink;
    }
}
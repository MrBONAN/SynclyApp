namespace Domain;

public class TrackInfo
{
    public Track CurrentTrack { get; set; }
    public Tuple<Track> TopTenTracks { get; set; }
    public Tuple<string> TopTenArtists { get; set; }
}
namespace Domain;

public class Track
{
    public readonly string Id;
    public readonly string Name;
    public readonly string ArtistId;
    public readonly string Genre;
    public readonly string LinkOnPlatform;
    public readonly string CoverImage;

    public Track(string id, string name, string genre, string artistId, string linkOnPlatform, string coverImage)
    {
        Id = id;
        Name = name;
        Genre = genre;
        ArtistId = artistId;
        LinkOnPlatform = linkOnPlatform;
        CoverImage = coverImage;
    }
}
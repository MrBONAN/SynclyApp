namespace Domain;

public class Artist
{
    public readonly string Id;
    public readonly string Name;
    public readonly string ProfileImageURL;

    public Artist(string id, string profileImageURL, string name)
    {
        Id = id;
        Name = name;
        ProfileImageURL = profileImageURL;
    }
}
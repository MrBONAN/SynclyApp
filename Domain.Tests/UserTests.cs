using Xunit;

namespace Domain.Tests;

public class UserTests
{
    [Fact]
    public void Constructor_WhenCalled_SetsIdAndName()
    {
        // Arrange
        string expectedId = "123";
        string expectedName = "Test User";

        // Act
        var user = new User(expectedId, expectedName);

        // Assert
        Assert.Equal(expectedId, user.Id);
        Assert.Equal(expectedName, user.Name);
    }

    [Fact]
    public void AddApp_WhenCalled_AddsAppToMusicAppLinks()
    {
        // Arrange
        var user = new User("123", "Test User");
        string app = "Spotify";
        string link = "spotify:user:123";

        // Act
        user.AddApp(app, link);

        // Assert
        Assert.Equal(link, user.MusicAppLinks[app]);
    }

    [Fact]
    public void User_Properties_CanBeSetAndRetrieved()
    {
        // Arrange
        var user = new User("123", "Test User");
        var location = new Location(1.0, 1.0);
        var tracks = new List<Track> { new Track("1", "Test Track", new List<Artist>()) };
        var artists = new List<Artist> { new Artist("1", "Test Artist") };

        // Act
        user.Location = location;
        user.TopTracks = tracks;
        user.TopArtists = artists;

        // Assert
        Assert.Equal(location, user.Location);
        Assert.Equal(tracks, user.TopTracks);
        Assert.Equal(artists, user.TopArtists);
    }
}

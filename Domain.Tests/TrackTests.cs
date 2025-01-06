namespace Domain.Tests;

public class TrackTests
{
    [Fact]
    public void Constructor_WhenCalled_SetsProperties()
    {
        // Arrange
        var id = "123";
        var name = "Test Track";
        var artists = new List<Artist> { new("1", "Test Artist") };

        // Act
        var track = new Track(id, name, artists);

        // Assert
        Assert.Equal(id, track.Id);
        Assert.Equal(name, track.Name);
        Assert.Equal(artists, track.Artists);
    }

    [Fact]
    public void Constructor_WhenArtistsIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var id = "123";
        var name = "Test Track";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Track(id, name, null));
    }

    [Fact]
    public void Constructor_WhenIdIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var name = "Test Track";
        var artists = new List<Artist> { new("1", "Test Artist") };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Track(null, name, artists));
    }

    [Fact]
    public void Constructor_WhenNameIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var id = "123";
        var artists = new List<Artist> { new("1", "Test Artist") };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Track(id, null, artists));
    }
}

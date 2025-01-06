namespace Domain.Tests;

public class ArtistTests
{
    [Fact]
    public void Constructor_WhenCalled_SetsProperties()
    {
        // Arrange
        var id = "123";
        var name = "Test Artist";

        // Act
        var artist = new Artist(id, name);

        // Assert
        Assert.Equal(id, artist.Id);
        Assert.Equal(name, artist.Name);
    }

    [Fact]
    public void Constructor_WhenIdIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var name = "Test Artist";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Artist(null, name));
    }

    [Fact]
    public void Constructor_WhenNameIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var id = "123";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Artist(id, null));
    }

    [Fact]
    public void Equals_WhenSameId_ReturnsTrue()
    {
        // Arrange
        var artist1 = new Artist("123", "Artist 1");
        var artist2 = new Artist("123", "Artist 2");

        // Act
        var result = artist1.Equals(artist2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WhenDifferentId_ReturnsFalse()
    {
        // Arrange
        var artist1 = new Artist("123", "Artist 1");
        var artist2 = new Artist("456", "Artist 1");

        // Act
        var result = artist1.Equals(artist2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetHashCode_WhenSameId_ReturnsSameHashCode()
    {
        // Arrange
        var artist1 = new Artist("123", "Artist 1");
        var artist2 = new Artist("123", "Artist 2");

        // Act
        var hashCode1 = artist1.GetHashCode();
        var hashCode2 = artist2.GetHashCode();

        // Assert
        Assert.Equal(hashCode1, hashCode2);
    }
}

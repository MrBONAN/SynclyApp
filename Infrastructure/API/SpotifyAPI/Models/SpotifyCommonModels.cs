using System.Text.Json.Serialization;
using System.Text.Json;

namespace Infrastructure.API.SpotifyAPI.Models;

public class ExternalUrls
{
    [JsonPropertyName("spotify")] public string? Spotify { get; set; }
}

public class Image
{
    [JsonPropertyName("url")] public string? Url { get; set; }
    [JsonPropertyName("height")] public int Height { get; set; }
    [JsonPropertyName("width")] public int Width { get; set; }
}

public class Artist
{
    [JsonPropertyName("external_urls")] public ExternalUrls? ExternalUrls { get; set; }
    [JsonPropertyName("uri")] public string? Uri { get; set; }
    [JsonPropertyName("id")] public string? Id { get; set; }
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("genres")] public List<string>? Genres { get; set; }
    [JsonPropertyName("images")] public List<Image>? Images { get; set; }
    [JsonPropertyName("popularity")] public int? Popularity { get; set; }
}

public class Track
{
    [JsonPropertyName("external_urls")] public ExternalUrls? ExternalUrls { get; set; }
    [JsonPropertyName("uri")] public string? Uri { get; set; }
    [JsonPropertyName("id")] public string? Id { get; set; }
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("artists")] public List<Artist>? Artists { get; set; }
    [JsonPropertyName("popularity")] public int? Popularity { get; set; }
    
    [JsonIgnore] public List<Image>? Images { get; set; }

    [JsonInclude]
    [JsonPropertyName("album")]
    private JsonElement Album 
    {
        set
        {
            if (value.TryGetProperty("images", out var images))
                Images = JsonSerializer.Deserialize<List<Image>>(images);
        }
    }
}

public class TracksAndArtistsList<T>
{
    [JsonPropertyName("tracks")] public List<T>? Tracks { get; set; }
    [JsonPropertyName("artists")] public List<T>? Artists { get; set; }
}
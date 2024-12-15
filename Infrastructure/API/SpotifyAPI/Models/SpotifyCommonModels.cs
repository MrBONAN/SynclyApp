using System.Text.Json.Serialization;

namespace Infrastructure.API.SpotifyAPI.Models;

public class ExternalUrls
{
    [JsonPropertyName("spotify")] public string? Spotify { get; set; }
}

public class Image
{
    [JsonPropertyName("uri")] public string? Url { get; set; }
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
}
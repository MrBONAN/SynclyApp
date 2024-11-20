using System.Text.Json.Serialization;

namespace Infrastructure.API.SpotifyAPI;

public class AccessToken
{
    [JsonPropertyName("access_token")] public string? TokenValue { get; set; }
    [JsonPropertyName("token_type")] public string? TokenType { get; set; }
    [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; }
}

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
}

public class Track
{
    [JsonPropertyName("external_urls")] public ExternalUrls? ExternalUrls { get; set; }
    [JsonPropertyName("uri")] public string? Uri { get; set; }
    [JsonPropertyName("id")] public string? Id { get; set; }
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("artists")] public List<Artist>? Artists { get; set; }
}

public class TracksSearchResponse
{
    [JsonPropertyName("items")] public List<Track>? Items { get; set; }
}

public class ArtistsSearchResponse
{
    [JsonPropertyName("items")] public List<Artist>? Items { get; set; }
}

public class SearchResponse
{
    [JsonPropertyName("tracks")] public TracksSearchResponse? Tracks { get; set; }
    [JsonPropertyName("artists")] public ArtistsSearchResponse? Artists { get; set; }
}
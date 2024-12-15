using System.Text.Json.Serialization;

namespace Infrastructure.API.SpotifyAPI.Models;

public class AccessToken
{
    [JsonPropertyName("access_token")] public string? TokenValue { get; set; }
    [JsonPropertyName("token_type")] public string? TokenType { get; set; }
    [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; }
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
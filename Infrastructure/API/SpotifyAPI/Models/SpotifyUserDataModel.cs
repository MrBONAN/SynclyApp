using System.Text.Json.Serialization;

namespace Infrastructure.API.SpotifyAPI.Models;

public class UserProfile
{
    [JsonPropertyName("display_name")] public string? DisplayName { get; set; }
    [JsonPropertyName("external_urls")] public ExternalUrls? ExternalUrls { get; set; }
    [JsonPropertyName("id")] public string? Id { get; set; }
    [JsonPropertyName("images")] public List<Image>? Images { get; set; }
    [JsonPropertyName("type")] public string? Type { get; set; }
    [JsonPropertyName("uri")] public string? Uri { get; set; }
}
using System.Text.Json.Serialization;

namespace Infrastructure.API.SpotifyAPI.Models;

public class Paging<T>
{
    [JsonPropertyName("items")] public List<T>? Items { get; set; }
    [JsonPropertyName("total")] public int? Total { get; set; }
    [JsonPropertyName("limit")] public int? Limit { get; set; }
    [JsonPropertyName("offset")] public int? Offset { get; set; }
    [JsonPropertyName("previous")] public string? Previous { get; set; }
    [JsonPropertyName("next")] public string? Next { get; set; }
}
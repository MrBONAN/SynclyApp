using System.Text.Json.Serialization;

namespace Infrastructure.API.SpotifyAPI.Models
{
    public class PlaybackState
    {
        [JsonPropertyName("item")] public Track? Item { get; set; }

        [JsonPropertyName("currently_playing_type")] public string? CurrentlyPlayingType { get; set; }
    }
}
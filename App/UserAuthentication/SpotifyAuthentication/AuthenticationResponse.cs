using System.Text.Json.Serialization;

namespace App.UserAuthentication.SpotifyAuthentication;

public record AuthenticationResponse(AuthenticationResult Result, string? Code);

public enum AuthenticationResult
{
    Success,
    Error,
    Canceled
}

public record AuthenticationPkceResponse(
    AuthenticationResult Result,
    string? Code,
    string? CodeVerifier,
    string? ErrorInfo)
{
}

public enum AccessTokenResult
{
    Success,
    Error,
}

public class PkceAccessToken
{
    [JsonPropertyName("access_token")] public string? AccessToken { get; } = null;
    [JsonPropertyName("refresh_token")] public string? RefreshToken { get; } = null;
    [JsonPropertyName("expires_in")] public int? ExpiresIn { get; } = null;
    [JsonPropertyName("token_type")] public string? TokenType { get; } = null;

    [JsonIgnore] public AccessTokenResult Result { get; set; } = AccessTokenResult.Success;
    [JsonIgnore] private DateTime CreationTime { get; } = DateTime.Now;

    public bool IsExpired()
    {
        return DateTime.Now.TimeOfDay.TotalSeconds - CreationTime.TimeOfDay.Seconds > ExpiresIn;
    }
}
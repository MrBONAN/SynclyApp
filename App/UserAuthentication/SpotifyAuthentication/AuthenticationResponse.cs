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
    [JsonPropertyName("access_token")] public string? AccessToken { get; set; } = null;
    [JsonPropertyName("token_type")] public string? TokenType { get; set; } = null;
    [JsonPropertyName("expires_in")] public int? ExpiresIn { get; set; } = null;
    [JsonPropertyName("refresh_token")] public string? RefreshToken { get; set; } = null;

    [JsonIgnore] public AccessTokenResult Result { get; set; } = AccessTokenResult.Success;
    [JsonIgnore] private long CreationTimeSeconds { get; } = (long)TimeSpan.FromTicks(DateTime.Now.Ticks).TotalSeconds;

    public bool IsExpired()
    {
        return (long)TimeSpan.FromTicks(DateTime.Now.Ticks).TotalSeconds - CreationTimeSeconds > ExpiresIn;
    }
}
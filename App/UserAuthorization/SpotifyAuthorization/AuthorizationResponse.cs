using System.Text.Json.Serialization;

namespace App.UserAuthorization.SpotifyAuthorization;

public record AuthorizationResponse(AuthorizationResult Result, string? Code);

public enum AuthorizationResult
{
    Success,
    Error,
    Canceled
}

public record AuthorizationPkceResponse(
    AuthorizationResult Result,
    string? Code,
    string? CodeVerifier,
    string? ErrorInfo)
{
}

public enum PkceAccessTokenResult
{
    Success,
    ReceiveError,
    RefreshError
}

public class PkceAccessToken
{
    [JsonPropertyName("access_token")] public string? AccessToken { get; set; } = null;
    [JsonPropertyName("token_type")] public string? TokenType { get; set; } = null;
    [JsonPropertyName("expires_in")] public int? ExpiresIn { get; set; } = null;
    [JsonPropertyName("refresh_token")] public string? RefreshToken { get; set; } = null;

    [JsonIgnore] public PkceAccessTokenResult Result { get; set; } = PkceAccessTokenResult.Success;
    [JsonIgnore] private long CreationTimeSeconds { get; } = (long)TimeSpan.FromTicks(DateTime.Now.Ticks).TotalSeconds;

    public bool IsExpired()
    {
        return (long)TimeSpan.FromTicks(DateTime.Now.Ticks).TotalSeconds - CreationTimeSeconds > ExpiresIn;
    }
}

public enum AccessTokenResult
{
    Success,
    DataNotFoundError,
    DeserializeError,
    RefreshError,
    Error
}

public record AccessToken(string? Value, AccessTokenResult Result);
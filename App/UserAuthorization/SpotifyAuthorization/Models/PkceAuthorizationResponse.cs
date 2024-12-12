using System.Text.Json.Serialization;

namespace App.UserAuthorization.SpotifyAuthorization.Models;

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
    ExchangeError,
    RefreshError
}

public class PkceAccessToken
{
    [JsonPropertyName("access_token")] public string? AccessToken { get; set; } = null;
    [JsonPropertyName("token_type")] public string? TokenType { get; set; } = null;
    [JsonPropertyName("expires_in")] public int? ExpiresIn { get; set; } = null;
    [JsonPropertyName("refresh_token")] public string? RefreshToken { get; set; } = null;

    public PkceAccessTokenResult Result { get; set; } = PkceAccessTokenResult.Success;
    [JsonInclude] public long CreationTimeSeconds { get; }

    [JsonConstructor]
    public PkceAccessToken(
        string? accessToken,
        string? tokenType,
        int? expiresIn,
        string? refreshToken,
        PkceAccessTokenResult result,
        long creationTimeSeconds)
    {
        AccessToken = accessToken;
        TokenType = tokenType;
        ExpiresIn = expiresIn;
        RefreshToken = refreshToken;
        Result = result;
        CreationTimeSeconds = creationTimeSeconds > 0 ? creationTimeSeconds : DateTimeOffset.Now.ToUnixTimeSeconds();
    }

    public PkceAccessToken()
    {
    }

    public bool IsExpired()
    {
        var elapsedTime = DateTimeOffset.Now.ToUnixTimeSeconds() - CreationTimeSeconds;
        return elapsedTime > ExpiresIn - 10; // 10 секунд убрал на всяки случай, чтобы пока мы сохраням/обновляем данные
        // срок действия ключа не истёк
    }
}
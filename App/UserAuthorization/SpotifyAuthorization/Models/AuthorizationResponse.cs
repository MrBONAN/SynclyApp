namespace App.UserAuthorization.SpotifyAuthorization.Models;

public enum AccessTokenResult
{
    Success,
    DataNotFoundError,
    DeserializeError,
    RefreshError,
    Error
}

public record AccessToken(string? Value, AccessTokenResult Result);

public enum LogInResult
{
    Success,
    AuthorizationError,
    AuthorizationCancelation,
    ExchangeTokenError
}
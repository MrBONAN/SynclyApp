namespace App.UserAuthorization;

public enum LogInResult
{
    Success,
    AuthorizationError,
    AuthorizationCancelation,
    ExchangeTokenError
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
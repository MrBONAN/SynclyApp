using App.UserAuthorization.SpotifyAuthorization.Models;

namespace App.UserAuthorization.SpotifyAuthorization;

public interface ISpotifyAccessTokenService
{
    Task<AccessToken> GetAsync();
    void RemoveToken();
}

public interface ISpotifyAuthManager
{
    Task<LogInResult> LogInAsync();
    void LogOut();
}

public interface ISpotifyPkceAuthorizationService
{
    Task<AuthorizationPkceResponse> AuthorizeWithPkceAsync();
    Task<PkceAccessToken?> RefreshTokenAsync(string refreshToken);
    Task<PkceAccessToken> ExchangeCodeForPkceTokenAsync(string code, string codeVerifier);
}
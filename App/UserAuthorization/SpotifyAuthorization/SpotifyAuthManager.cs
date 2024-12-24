using System.Text.Json;
using App.UserAuthorization.SpotifyAuthorization.Models;

namespace App.UserAuthorization.SpotifyAuthorization;

public class SpotifyAuthManager(
    ISpotifyAccessTokenService accessTokenService,
    ISpotifyPkceAuthorizationService pkceAuthorizationService)
    : ISpotifyAuthManager
{
    public async Task<LogInResult> LogInAsync()
    {
        var authResponse = await pkceAuthorizationService.AuthorizeWithPkceAsync();
        if (authResponse.Result == AuthorizationResult.Canceled)
            return LogInResult.AuthorizationCancelation;

        if (authResponse.Result == AuthorizationResult.Error)
            return LogInResult.AuthorizationError;

        var accessToken = await pkceAuthorizationService.ExchangeCodeForPkceTokenAsync(authResponse.Code!, authResponse.CodeVerifier!);
        if (accessToken.Result == PkceAccessTokenResult.ExchangeError)
            return LogInResult.ExchangeTokenError;

        await SaveAccessTokenAsync(accessToken);
        return LogInResult.Success;
    }

    public void LogOut()
    {
        SecureStorage.Default.Remove("spotify_token");
        accessTokenService.RemoveToken();
    }
    
    private static async Task SaveAccessTokenAsync(PkceAccessToken accessToken)
    {
        var jsonAccessToken = JsonSerializer.Serialize(accessToken);
        await SecureStorage.Default.SetAsync("spotify_token", jsonAccessToken);
    }
}

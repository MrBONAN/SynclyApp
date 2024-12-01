using System.Text.Json;
using App.UserAuthorization.SpotifyAuthorization.Models;

namespace App.UserAuthorization.SpotifyAuthorization;

public static class SpotifyAuthManager
{
    public static async Task<LogInResult> LogIn()
    {
        var authResponse = await SpotifyPkceAuthorization.AuthorizeWithPkceAsync();
        if (authResponse.Result is AuthorizationResult.Canceled)
            return LogInResult.AuthorizationCancelation;
        if (authResponse.Result is AuthorizationResult.Error)
            return LogInResult.AuthorizationError;
        var accessToken = await SpotifyPkceAuthorization.ExchangeCodeForPkceTokenAsync(authResponse.Code!,
            authResponse.CodeVerifier!);
        if (accessToken.Result is PkceAccessTokenResult.ExchangeError)
            return LogInResult.ExchangeTokenError;
        await SaveAccessToken(accessToken);
        return LogInResult.Success;
    }

    public static async Task LogOut()
    {
        // TODO
    }

    private static async Task SaveAccessToken(PkceAccessToken accessToken)
    {
        var jsonAccessToken = JsonSerializer.Serialize(accessToken);
        await SecureStorage.Default.SetAsync("spotify_token", jsonAccessToken);
    }
}
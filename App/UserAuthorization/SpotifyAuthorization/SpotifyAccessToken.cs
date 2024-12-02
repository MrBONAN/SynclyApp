using System.Diagnostics;
using System.Text.Json;

namespace App.UserAuthorization.SpotifyAuthorization.Models;

public static class SpotifyAccessToken
{
    private static PkceAccessToken? PkceAccessToken { get; set; }

    public static async Task<AccessToken> Get()
    {
        if (PkceAccessToken is null)
        {
            var (pkceAccessToken, readingResult) = await ReadAccessToken();
            if (readingResult is not AccessTokenResult.Success)
                return new AccessToken(null, readingResult);
            PkceAccessToken = pkceAccessToken!;
        }

        if (true || PkceAccessToken.IsExpired())
        {
            var pkceAccessToken = await SpotifyPkceAuthorization.RefreshTokenAsync(PkceAccessToken.RefreshToken!);
            if (pkceAccessToken!.Result is PkceAccessTokenResult.RefreshError)
                return new AccessToken(null, AccessTokenResult.RefreshError);
            PkceAccessToken = pkceAccessToken;
            await SaveAccessToken(PkceAccessToken);
        }

        if (PkceAccessToken.Result is not PkceAccessTokenResult.Success)
        {
            Debug.WriteLine("Произошла ошибка в логике получения токина доступа");
            return new AccessToken(null, AccessTokenResult.Error);
        }

        return new AccessToken(PkceAccessToken.AccessToken, AccessTokenResult.Success);
    }

    private static async Task<(PkceAccessToken?, AccessTokenResult)> ReadAccessToken()
    {
        var json = await SecureStorage.Default.GetAsync("spotify_token");
        if (string.IsNullOrWhiteSpace(json))
            return (null, AccessTokenResult.DataNotFoundError);
        var pkceAccessToken = JsonSerializer.Deserialize<PkceAccessToken>(json);
        if (pkceAccessToken is null)
            return (null, AccessTokenResult.DeserializeError);
        return (pkceAccessToken, AccessTokenResult.Success);
    }

    private static async Task SaveAccessToken(PkceAccessToken accessToken)
    {
        var jsonAccessToken = JsonSerializer.Serialize(accessToken);
        await SecureStorage.Default.SetAsync("spotify_token", jsonAccessToken);
    }
}
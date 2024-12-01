using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Infrastructure.API.SpotifyAPI;
using RestSharp;
using App.UserAuthorization.SpotifyAuthorization.Models;

namespace App.UserAuthorization.SpotifyAuthorization;

public static class SpotifyPkceAuthorization
{
    private static readonly string ClientId = SpotifyApi.ClientId;
    private static string RedirectUri = "syncly-auth://callback";
    private static string AuthorizeUrl = "https://accounts.spotify.com/authorize";
    private static string Scope = "user-read-private user-read-email";

    public static async Task<AuthorizationPkceResponse> AuthorizeWithPkceAsync()
    {
        var codeVerifier = GenerateCodeVerifier();
        var codeChallenge = GenerateCodeChallengeBase64(codeVerifier);

        var authUrl = new RestClient(AuthorizeUrl)
            .BuildUri(new RestRequest()
                .AddParameter("client_id", ClientId)
                .AddParameter("response_type", "code")
                .AddParameter("redirect_uri", RedirectUri)
                .AddParameter("scope", Scope)
                .AddParameter("code_challenge_method", "S256")
                .AddParameter("code_challenge", codeChallenge)
            );

        var options = new WebAuthenticatorOptions
        {
            Url = authUrl,
            CallbackUrl = new Uri(RedirectUri)
        };
        try
        {
            Debug.WriteLine("Авторизация начата с PKCE.");
            var result = await WebAuthenticator.Default.AuthenticateAsync(options);

            if (result.Properties.TryGetValue("code", out var code))
            {
                Debug.WriteLine("Авторизация завершена!");
                return new AuthorizationPkceResponse(AuthorizationResult.Success, code, codeVerifier, null);
            }

            Debug.WriteLine("Не удалось получить код авторизации.");
            result.Properties.TryGetValue("error", out var errorInfo);
            return new AuthorizationPkceResponse(AuthorizationResult.Error, null, codeVerifier, errorInfo);
        }
        catch (TaskCanceledException)
        {
            Debug.WriteLine("Пользователь отменил авторизацию или произошла ошибка.");
            return new AuthorizationPkceResponse(AuthorizationResult.Canceled, null, codeVerifier, null);
        }
    }


    private static string GenerateCodeVerifier(int length = 128)
    {
        var possible = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_.-~";
        var codeVerifier = new StringBuilder(length);
        var random = new Random();
        for (var i = 0; i < length; i++)
            codeVerifier.Append(possible[random.Next(possible.Length)]);
        return codeVerifier.ToString();
    }

    private static string GenerateCodeChallengeBase64(string codeVerifier)
    {
        using var sha256 = SHA256.Create();
        var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
        return Convert.ToBase64String(challengeBytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public static async Task<PkceAccessToken> ExchangeCodeForPkceTokenAsync(string code, string codeVerifier)
    {
        var client = new RestClient("https://accounts.spotify.com/api/token");

        var request = new RestRequest()
            .AddHeader("Content-Type", "application/x-www-form-urlencoded")
            .AddParameter("grant_type", "authorization_code")
            .AddParameter("code", code)
            .AddParameter("redirect_uri", RedirectUri)
            .AddParameter("client_id", ClientId)
            .AddParameter("code_verifier", codeVerifier);

        var response = await client.ExecutePostAsync<PkceAccessToken>(request);

        if (response.IsSuccessful && response.Data != null)
        {
            Debug.WriteLine("Токен успешно получен.");
            return response.Data;
        }

        Debug.WriteLine($"Ошибка при получении токена: {response.ErrorMessage ?? response.Content}");
        return new PkceAccessToken() { Result = PkceAccessTokenResult.ExchangeError };
    }

    public static async Task<PkceAccessToken?> RefreshTokenAsync(string refreshToken)
    {
        var client = new RestClient("https://accounts.spotify.com/api/token");

        var request = new RestRequest()
            .AddHeader("Content-Type", "application/x-www-form-urlencoded")
            .AddParameter("grant_type", "refresh_token")
            .AddParameter("refresh_token", refreshToken)
            .AddParameter("client_id", ClientId);

        var response = await client.ExecutePostAsync<PkceAccessToken>(request);

        if (response.IsSuccessful && response.Data != null)
        {
            Console.WriteLine("Токен успешно обновлён");
            return response.Data;
        }

        Console.WriteLine($"Ошибка при обновлении токена: {response.ErrorMessage ?? response.Content}");
        return new PkceAccessToken() { Result = PkceAccessTokenResult.RefreshError };
    }
}
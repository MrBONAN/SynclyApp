using Microsoft.Maui.Authentication;
using System.Diagnostics;
using Infrastructure.API.SpotifyAPI;
using RestSharp;

namespace App.UserAuthorization.SpotifyAuthorization;

public class SpotifyFlowCodeAuthorization
{
    private string ClientId = SpotifyApi.ClientId;
    private const string RedirectUri = "syncly-auth://callback";
    private const string AuthorizeUrl = "https://accounts.spotify.com/authorize";
    private const string Scope = "user-read-private user-read-email";

    private async Task<AuthorizationResponse> AuthenticateAsync()
    {
        try
        {
            var authUrl = new RestClient(AuthorizeUrl)
                .BuildUri(new RestRequest()
                    .AddParameter("client_id", ClientId)
                    .AddParameter("response_type", "code")
                    .AddParameter("redirect_uri", RedirectUri)
                    .AddParameter("scope", Scope)
                );

            // authUrl = $"{SpotifyAuthorizeUrl}?client_id={ClientId}&response_type=code&redirect_uri={Uri.EscapeDataString(RedirectUri)}&scope={Uri.EscapeDataString(Scope)}";
            var options = new WebAuthenticatorOptions
            {
                Url = authUrl,
                CallbackUrl = new Uri(RedirectUri)
            };

            Debug.WriteLine("Аутентификация начата.");
            var result = await WebAuthenticator.Default.AuthenticateAsync(options);
            Debug.WriteLine("Аутентификация завершена!");

            if (result.Properties.TryGetValue("code", out var code))
            {
                return new AuthorizationResponse(AuthorizationResult.Success, code);
            }

            Debug.WriteLine("Не удалось получить код авторизации.");
            return new AuthorizationResponse(AuthorizationResult.Error, null);
        }
        catch (TaskCanceledException)
        {
            Debug.WriteLine("Пользователь отменил аутентификацию или произошла ошибка.");
            return new AuthorizationResponse(AuthorizationResult.Canceled, null);
        }
    }

    private async Task ExchangeCodeForTokenAsync(string code)
    {
        // Логика обмена кода авторизации на токен
        try
        {
            // Формируем HTTP-запрос для обмена кода на токен
            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "client_id", ClientId },
                    { "grant_type", "authorization_code" },
                    { "code", code },
                    { "redirect_uri", RedirectUri }
                })
            };

            using var httpClient = new HttpClient();
            var response = await httpClient.SendAsync(tokenRequest);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"Токен успешно получен: {responseContent}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка при обмене кода на токен: {ex.Message}");
        }
    }
}
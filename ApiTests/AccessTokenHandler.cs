// using System.Security.Authentication;
// using System.Text;
// using App.UserAuthorization.SpotifyAuthorization.Models;
// using Infrastructure.API.SpotifyAPI;
// using RestSharp;
// using System.Text.Json;
//
// namespace ApiTests;
//
// public class AccessTokenHandler
// {
//     private static readonly string ClientId = SpotifyApi.ClientId;
//     private PkceAccessToken? accessToken;
//     private string pathAccessToken = "../../../AccessToken.env";
//
//     public string GetAccessToken()
//     {
//         if (accessToken is null)
//         {
//             var (token, result) = ReadAccessToken();
//             if (result is not AccessTokenResult.Success)
//                 throw new AuthenticationException("Cannot read accessToken from file");
//             accessToken = token!;
//         }
//
//         if (accessToken.IsExpired())
//         {
//             var refreshToken = RefreshAccessToken(accessToken.RefreshToken!);
//             if (refreshToken!.Result is not PkceAccessTokenResult.Success)
//                 throw new AuthenticationException("Cannot refresh access token");
//             WriteAccessToken();
//             accessToken = refreshToken;
//         }
//
//         return accessToken.AccessToken!;
//     }
//
//     private PkceAccessToken RefreshAccessToken(string refreshToken)
//     {
//         var client = new RestClient("https://accounts.spotify.com/api/token");
//         var request = new RestRequest()
//             .AddHeader("Content-Type", "application/x-www-form-urlencoded")
//             .AddParameter("grant_type", "refresh_token")
//             .AddParameter("refresh_token", refreshToken)
//             .AddParameter("client_id", ClientId);
//         var response = client.ExecutePostAsync<PkceAccessToken>(request).Result;
//         if (response.IsSuccessful && response.Data != null)
//             return response.Data;
//         return new PkceAccessToken { Result = PkceAccessTokenResult.RefreshError };
//     }
//
//     private (PkceAccessToken?, AccessTokenResult) ReadAccessToken()
//     {
//         var jsonAccessToken = File.ReadAllText(pathAccessToken);
//
//         var pkceAccessToken = JsonSerializer.Deserialize<PkceAccessToken>(jsonAccessToken);
//         return pkceAccessToken is not null
//             ? (pkceAccessToken, AccessTokenResult.Success)
//             : (null, AccessTokenResult.DeserializeError);
//     }
//
//     private void WriteAccessToken()
//     {
//         var jsonAccessToken = JsonSerializer.Serialize(accessToken);
//         File.WriteAllText(pathAccessToken, jsonAccessToken);
//     }
// }
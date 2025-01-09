// using System.Diagnostics;
// using System.Text.Json;
// using App.UserAuthorization.SpotifyAuthorization.Models;
//
// namespace App.UserAuthorization.SpotifyAuthorization;
//
// public class SpotifyAccessTokenService(ISpotifyPkceAuthorizationService pkceAuthorizationService)
//     : ISpotifyAccessTokenService
// {
//     private PkceAccessToken? pkceAccessToken;
//
//     public async Task<AccessToken> GetAsync()
//     {
//         if (pkceAccessToken == null)
//         {
//             var (pkceAccessToken, readingResult) = await ReadAccessTokenAsync();
//             if (readingResult != AccessTokenResult.Success)
//                 return new AccessToken(null, readingResult);
//
//             this.pkceAccessToken = pkceAccessToken;
//         }
//
//         if (pkceAccessToken.IsExpired())
//         {
//             var pkceAccessToken = await pkceAuthorizationService.RefreshTokenAsync(this.pkceAccessToken.RefreshToken!);
//             if (pkceAccessToken?.Result == PkceAccessTokenResult.RefreshError)
//                 return new AccessToken(null, AccessTokenResult.RefreshError);
//
//             this.pkceAccessToken = pkceAccessToken;
//             await SaveAccessTokenAsync(this.pkceAccessToken);
//         }
//
//         return new AccessToken(pkceAccessToken.AccessToken, AccessTokenResult.Success);
//     }
//
//     public void RemoveToken() => pkceAccessToken = null;
//
//     private static async Task<(PkceAccessToken?, AccessTokenResult)> ReadAccessTokenAsync()
//     {
//         var json = await SecureStorage.Default.GetAsync("spotify_token");
//         if (string.IsNullOrWhiteSpace(json))
//             return (null, AccessTokenResult.DataNotFoundError);
//
//         var pkceAccessToken = JsonSerializer.Deserialize<PkceAccessToken>(json);
//         return pkceAccessToken != null 
//             ? (pkceAccessToken, AccessTokenResult.Success) 
//             : (null, AccessTokenResult.DeserializeError);
//     }
//
//     private static async Task SaveAccessTokenAsync(PkceAccessToken accessToken)
//     {
//         var jsonAccessToken = JsonSerializer.Serialize(accessToken);
//         await SecureStorage.Default.SetAsync("spotify_token", jsonAccessToken);
//     }
// }

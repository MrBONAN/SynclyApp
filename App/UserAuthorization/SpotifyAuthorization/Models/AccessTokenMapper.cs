using Infrastructure.API.ServerApi.Models.Authorize;

namespace App.UserAuthorization.SpotifyAuthorization.Models;

public static class AccessTokenMapper
{
    public static SpotifyAccessTokenDto ToSpotifyAccessTokenDto(this PkceAccessToken pkceAccessToken)
    {
        var expiresDate = DateTime.UtcNow.AddSeconds(pkceAccessToken.ExpiresIn!.Value);
        return new SpotifyAccessTokenDto()
        {
            AccessToken = pkceAccessToken.AccessToken!,
            TokenType = pkceAccessToken.TokenType!,
            ExpiresDate = expiresDate,
            RefreshToken = pkceAccessToken.RefreshToken!
        };
    }
}
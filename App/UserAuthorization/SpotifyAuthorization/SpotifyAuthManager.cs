using System.Text.Json;
using App.UserAuthorization.SpotifyAuthorization.Models;
using Infrastructure.API.ServerApi;
using Infrastructure.API.ServerApi.Models.User;

namespace App.UserAuthorization.SpotifyAuthorization;

public class SpotifyAuthManager(
    IUserDataHandler userDataHandler,
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

        var spotifyAccessTokenDto = accessToken.ToSpotifyAccessTokenDto();
        var response = await ServerApi.AuthorizeSpotifyAsync(spotifyAccessTokenDto);
        if (response.Result is ApiResult.Ok)
        {
            await userDataHandler.SaveUserDataAsync(response.Data!);
            return LogInResult.Success;
        }
        // TODO
        return LogInResult.AuthorizationError;
    }
}

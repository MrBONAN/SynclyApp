using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace App.UserAuthorization.YandexAuthorization;

public class YandexAccessToken
{
    [JsonPropertyName("access_token")] public string? AccessToken { get; set; }
    [JsonPropertyName("expires_in")] public long? ExpiresIn { get; set; }
    [JsonPropertyName("result")] public AccessTokenResult Result { get; set; } = AccessTokenResult.Success;
    [JsonPropertyName("creation_time")] public long? CreationTimeSeconds { get; set; }

    private static YandexAccessToken? Token = null;

    [JsonConstructor]
    public YandexAccessToken(AccessTokenResult result, string? accessToken = null, long? expiresIn = null,
        long? creationTimeSeconds = null)
    {
        AccessToken = accessToken;
        ExpiresIn = expiresIn;
        Result = result;
        CreationTimeSeconds = creationTimeSeconds;
    }

    public YandexAccessToken(string url)
    {
        var pattern =
            @"access_token=(?<accessToken>[\w\d]+)&token_type=(?<tokenType>\w+)&expires_in=(?<expiresIn>\d+)&cid=(?<cid>\d+)";
        var match = Regex.Match(url, pattern);

        if (!match.Success)
        {
            Result = AccessTokenResult.Error;
            return;
        }

        AccessToken = match.Groups["accessToken"].Value;
        ExpiresIn = long.Parse(match.Groups["expiresIn"].Value);
        //var tokenType = match.Groups["tokenType"].Value;
        //var cid = match.Groups["cid"].Value;
        CreationTimeSeconds = DateTimeOffset.Now.ToUnixTimeSeconds();
        Result = AccessTokenResult.Success;
    }

    public static async Task<AccessToken> Get()
    {
        if (Token is null)
        {
            var (pkceAccessToken, readingResult) = await ReadAccessToken();
            if (readingResult is not AccessTokenResult.Success)
                return new AccessToken(null, readingResult);
            Token = pkceAccessToken!;
        }

        if (Token.IsExpired())
            return new AccessToken(null, AccessTokenResult.RefreshError);

        if (Token.Result is not AccessTokenResult.Success)
        {
            Debug.WriteLine("Произошла ошибка в логике получения токина доступа");
            return new AccessToken(null, AccessTokenResult.Error);
        }

        return new AccessToken(Token.AccessToken, AccessTokenResult.Success);
    }


    private static async Task<(YandexAccessToken?, AccessTokenResult)> ReadAccessToken()
    {
        var json = await SecureStorage.Default.GetAsync("yandex_token");
        if (string.IsNullOrWhiteSpace(json))
            return (null, AccessTokenResult.DataNotFoundError);
        var accessToken = JsonSerializer.Deserialize<YandexAccessToken>(json);
        if (accessToken is null)
            return (null, AccessTokenResult.DeserializeError);
        return (accessToken, AccessTokenResult.Success);
    }

    private bool IsExpired()
    {
        var elapsedTime = DateTimeOffset.Now.ToUnixTimeSeconds() - CreationTimeSeconds;
        return elapsedTime > ExpiresIn - 10;
    }
}
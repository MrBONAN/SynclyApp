using System.Text.Json;

namespace App.UserAuthorization.YandexAuthorization;

public partial class YandexAuthorizationPage : ContentPage
{
    private readonly string RedirectUri = "syncly-auth://callback";

    private readonly string YandexAuthorizeUrl =
        "https://oauth.yandex.ru/authorize?response_type=token&client_id=23cabbbdc6cd418abb4b39c32c41195d";

    private readonly TaskCompletionSource<LogInResult> TokenResultTask;

    public YandexAuthorizationPage(TaskCompletionSource<LogInResult> tokenResultTask)
    {
        InitializeComponent();

        TokenResultTask = tokenResultTask;
        AuthWebView.Source = YandexAuthorizeUrl;
        //Clear all the Application Cache, Web SQL Database and the HTML5 Web Storage
        // WebStorage.Instance.DeleteAllData();
        // CookieManager.Instance.RemoveAllCookies(null);
        // CookieManager.Instance.Flush();
    }

    private async void OnWebViewNavigating(object sender, WebNavigatingEventArgs e)
    {
        var url = e.Url;

        if (url.Contains("access_token"))
        {
            e.Cancel = true;
            var accessToken = new YandexAccessToken(url);
            var result = LogInResult.AuthorizationError;
            if (accessToken.Result is AccessTokenResult.Success)
            {
                result = LogInResult.Success;
                await SaveAccessToken(accessToken);
            }
            TokenResultTask.TrySetResult(result);
            await Navigation.PopModalAsync();
            return;
        }

        e.Cancel = true;
        TokenResultTask.TrySetResult(LogInResult.AuthorizationError);
        await Navigation.PopModalAsync();
    }

    private async Task SaveAccessToken(YandexAccessToken accessToken)
    {
        var jsonAccessToken = JsonSerializer.Serialize(accessToken);
        await SecureStorage.Default.SetAsync("yandex_token", jsonAccessToken);
    }
}
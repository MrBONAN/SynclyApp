using App.UserAuthorization;
using App.UserAuthorization.SpotifyAuthorization;
using Microsoft.Extensions.Logging;
namespace App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });
        
        // Регистрация страниц
        builder.Services.AddTransient<MainPage>();
        
        // Регистрация интерфейсов для работы со Spotify
        builder.Services.AddSingleton<IUserDataHandler, UserDataHandler>();
        builder.Services.AddSingleton<ISpotifyAuthManager, SpotifyAuthManager>();
        builder.Services.AddSingleton<ISpotifyPkceAuthorizationService, SpotifyPkceAuthorizationService>();
        
#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
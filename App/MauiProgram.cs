using CommunityToolkit.Maui;
using App.UserAuthorization.SpotifyAuthorization;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using The49.Maui.BottomSheet;
using CommunityToolkit.Maui;

namespace App;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>().UseMauiCommunityToolkit().UseBottomSheet().ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            fonts.AddFont("Roboto-Regular.ttf", "RobotoRegular");
        }).ConfigureLifecycleEvents(events =>
        {
#if ANDROID
                events.AddAndroid(android => android.OnCreate((activity, bundle) => MakeStatusBarTranslucent(activity)));

                static void MakeStatusBarTranslucent(Android.App.Activity activity)
                {
                    activity.Window.SetFlags(Android.Views.WindowManagerFlags.LayoutNoLimits, Android.Views.WindowManagerFlags.LayoutNoLimits);

                    activity.Window.ClearFlags(Android.Views.WindowManagerFlags.TranslucentStatus);

                    activity.Window.SetStatusBarColor(Android.Graphics.Color.Transparent);
                }
#endif
        }).UseMauiCommunityToolkitMediaElement();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<ProfileBottomSheet.Sheet>();
        builder.Services.AddTransient<SettingsBottomSheet>();
        builder.Services.AddTransient<ConnectionsBottomSheet.Sheet>();
        builder.Services.AddTransient<SignIn>();
        builder.Services.AddTransient<AppearanceBottomSheet.Sheet>();
        builder.Services.AddTransient<SignIn>();
        builder.Services.AddTransient<Map>();
        builder.Services.AddSingleton<MapCommands>();
        builder.Services.AddSingleton<ISpotifyAccessTokenService, SpotifyAccessTokenService>();
        builder.Services.AddSingleton<ISpotifyAuthManager, SpotifyAuthManager>();
        builder.Services.AddSingleton<ISpotifyPkceAuthorizationService, SpotifyPkceAuthorizationService>();
#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}
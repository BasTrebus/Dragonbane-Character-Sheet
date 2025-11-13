using Microsoft.Extensions.Logging;
using System;

namespace DragonbaneCharacterSheetGenerator
{
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
                });

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            // Local file import service for adding JSON docs at runtime
            builder.Services.AddSingleton<Services.ILocalDocService, Services.LocalDocService>();

            // Theme service (uses JS runtime to apply theme class)
            builder.Services.AddScoped<Services.IThemeService, Services.ThemeService>();

            // Favorites service
            builder.Services.AddSingleton<Services.IFavoritesService, Services.FavoritesService>();

            return builder.Build();
        }
    }
}

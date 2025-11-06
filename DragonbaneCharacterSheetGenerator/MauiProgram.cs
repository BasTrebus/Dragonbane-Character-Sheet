using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;

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

            // Provide an HttpClient with a BaseAddress so relative URIs work in injected HttpClient
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost/") });

            // Local file import service for adding JSON docs at runtime
            builder.Services.AddSingleton<Services.ILocalDocService, Services.LocalDocService>();

            return builder.Build();
        }
    }
}

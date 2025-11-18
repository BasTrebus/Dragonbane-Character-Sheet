using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Web;
using System.Threading.Tasks;

namespace DragonbaneCharacterSheetGenerator.Components.Pages
{
    public partial class Settings
    {
        [Inject]
        public required DragonbaneCharacterSheetGenerator.Services.IThemeService ThemeService { get; set; }

        [Inject]
        public required DragonbaneCharacterSheetGenerator.Services.ILocalDocService LocalDocService { get; set; }

        [Inject]
        public required DragonbaneCharacterSheetGenerator.Services.IFavoritesService FavoritesService { get; set; }

        [Inject]
        public required IJSRuntime JS { get; set; }

        private string CurrentTheme { get; set; } = "light";

        protected override async Task OnInitializedAsync()
        {
            CurrentTheme = await ThemeService.GetThemeAsync();
        }

        private async Task ToggleTheme()
        {
            await ThemeService.ToggleThemeAsync();
            CurrentTheme = await ThemeService.GetThemeAsync();
        }

        private async Task SetThemeAsync(string theme)
        {
            CurrentTheme = theme;
            await ThemeService.SetThemeAsync(theme);
        }

        private Task SetLight() => SetThemeAsync("light");
        private Task SetDark() => SetThemeAsync("dark");
        private Task SetSystem() => SetThemeAsync("system");

        private async Task ImportJson()
        {
            var (ok, msg) = await LocalDocService.ImportJsonAsync();
            await JS.InvokeVoidAsync("alert", ok ? msg : "Import failed: " + msg);
        }

        private async Task ClearFavorites()
        {
            await FavoritesService.ClearAllAsync();
            await JS.InvokeVoidAsync("alert", "All favorites removed.");
        }
    }
}

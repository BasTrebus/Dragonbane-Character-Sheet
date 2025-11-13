using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
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

        private async Task ToggleTheme()
        {
            await ThemeService.ToggleThemeAsync();
        }

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

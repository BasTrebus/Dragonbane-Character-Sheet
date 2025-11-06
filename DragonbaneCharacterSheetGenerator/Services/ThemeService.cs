using System.Threading.Tasks;
using Microsoft.JSInterop;
using Microsoft.Maui.Storage;

namespace DragonbaneCharacterSheetGenerator.Services
{
    public interface IThemeService
    {
        Task<string> GetThemeAsync();
        Task SetThemeAsync(string theme);
        Task ToggleThemeAsync();
    }

    public class ThemeService : IThemeService
    {
        private const string PrefKey = "theme";
        private readonly IJSRuntime _js;

        public ThemeService(IJSRuntime js)
        {
            _js = js;
        }

        public async Task<string> GetThemeAsync()
        {
            var stored = Preferences.Get(PrefKey, string.Empty);
            if (!string.IsNullOrWhiteSpace(stored)) return stored;

            // ask JS for preferred scheme (falls back to 'light')
            try
            {
                var detected = await _js.InvokeAsync<string>("themeInterop.getPreferredScheme");
                if (!string.IsNullOrWhiteSpace(detected)) return detected;
            }
            catch
            {
                // ignore
            }

            return "light";
        }

        public async Task SetThemeAsync(string theme)
        {
            if (string.IsNullOrWhiteSpace(theme)) theme = "light";
            Preferences.Set(PrefKey, theme);
            try
            {
                await _js.InvokeVoidAsync("themeInterop.applyTheme", theme);
            }
            catch
            {
                // ignore JS failures
            }
        }

        public async Task ToggleThemeAsync()
        {
            var current = await GetThemeAsync();
            var next = current == "dark" ? "light" : "dark";
            await SetThemeAsync(next);
        }
    }
}

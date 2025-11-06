using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using DragonbaneCharacterSheetGenerator.Shared;

namespace DragonbaneCharacterSheetGenerator.Components.Pages
{
    public class HeroicAbilitiesBase : ComponentBase
    {
        protected List<HeroicAbility>? abilities;
        protected string? footerNote;
        protected string searchTerm = string.Empty;

        protected HashSet<string> favorites = new();
        protected bool showFavoritesOnly = false;

        protected string KeyFor(string kind, string? name) => $"{kind}|{(name ?? string.Empty).Trim()}";
        protected bool IsFavorite(string kind, string? name) => favorites.Contains(KeyFor(kind, name));
        protected void ToggleFavorite(string kind, string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            var k = KeyFor(kind, name);
            if (favorites.Contains(k)) favorites.Remove(k); else favorites.Add(k);
            Preferences.Set("favorites", JsonSerializer.Serialize(favorites));
            StateHasChanged();
        }
        protected void LoadFavorites()
        {
            try
            {
                var json = Preferences.Get("favorites", "[]");
                favorites = JsonSerializer.Deserialize<HashSet<string>>(json) ?? new HashSet<string>();
            }
            catch { favorites = new HashSet<string>(); }
        }

        protected IEnumerable<HeroicAbility> FilteredAbilities =>
            (abilities ?? Enumerable.Empty<HeroicAbility>())
                .Where(a =>
                    (string.IsNullOrWhiteSpace(searchTerm) ||
                        (a.Name ?? string.Empty).Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        (a.Skill ?? string.Empty).Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        (a.Description ?? string.Empty).Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    && (!showFavoritesOnly || IsFavorite("heroic", a.Name))
                );

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var overridePath = Path.Combine(FileSystem.AppDataDirectory, "doc", "heroicAbilities.json");
                if (File.Exists(overridePath))
                {
                    await using var stream = File.OpenRead(overridePath);
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var doc = await JsonSerializer.DeserializeAsync<HeroicAbilitiesDoc>(stream, options);

                    abilities = doc?.Abilities ?? new List<HeroicAbility>();
                    footerNote = doc?.FooterNote ?? string.Empty;
                }
                else
                {
                    await using var stream = await FileSystem.OpenAppPackageFileAsync("wwwroot/doc/heroicAbilities.json");
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var doc = await JsonSerializer.DeserializeAsync<HeroicAbilitiesDoc>(stream, options);

                    abilities = doc?.Abilities ?? new List<HeroicAbility>();
                    footerNote = doc?.FooterNote ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                abilities = new List<HeroicAbility>();
                footerNote = $"Error loading abilities: {ex.GetType().Name}: {ex.Message}";
            }

            LoadFavorites();
        }

        protected void ToggleFavoriteHeroic(string? name) => ToggleFavorite("heroic", name);
        protected bool IsFavoriteHeroic(string? name) => IsFavorite("heroic", name);

        protected int CountFavoritesForKind(string kind)
        {
            if (favorites == null || favorites.Count == 0) return 0;
            return favorites.Count(f => f != null && f.StartsWith($"{kind}|", StringComparison.OrdinalIgnoreCase));
        }
    }
}

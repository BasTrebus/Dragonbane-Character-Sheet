using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Maui.Storage;

namespace DragonbaneCharacterSheetGenerator.Components.Pages
{
    // Use a base class for component logic. The .razor file will inherit this class.
    public class InnateAbilitiesBase : ComponentBase
    {
        protected List<InnateAbility>? abilities;
        protected string searchTerm = string.Empty;

        protected HashSet<string> favorites = new();
        protected bool showFavoritesOnly = false;

        protected IEnumerable<InnateAbility> FilteredAbilities =>
            (abilities ?? Enumerable.Empty<InnateAbility>())
            .Where(a =>
                (string.IsNullOrWhiteSpace(searchTerm) ||
                 (a.Kin ?? string.Empty).Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                 (a.Name ?? string.Empty).Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                 (a.Description ?? string.Empty).Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                && (!showFavoritesOnly || IsFavorite("innate", a.Name))
            );

    protected override async Task OnInitializedAsync()
        {
            try
            {
                var overridePath = Path.Combine(FileSystem.AppDataDirectory, "doc", "innateAbilities.json");
                if (File.Exists(overridePath))
                {
                    await using var stream = File.OpenRead(overridePath);
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var kinList = await JsonSerializer.DeserializeAsync<List<KinEntry>>(stream, options);

                    abilities = kinList?.SelectMany(k => (k.Abilities ?? new List<InnateAbilitySource>()).Select(a => new InnateAbility
                    {
                        Kin = k.Kin,
                        Name = a.Name,
                        Wp = a.Wp,
                        Description = a.Description
                    })).ToList() ?? new List<InnateAbility>();
                }
                else
                {
                    await using var stream = await FileSystem.OpenAppPackageFileAsync("wwwroot/doc/innateAbilities.json");
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var kinList = await JsonSerializer.DeserializeAsync<List<KinEntry>>(stream, options);

                    abilities = kinList?.SelectMany(k => (k.Abilities ?? new List<InnateAbilitySource>()).Select(a => new InnateAbility
                    {
                        Kin = k.Kin,
                        Name = a.Name,
                        Wp = a.Wp,
                        Description = a.Description
                    })).ToList() ?? new List<InnateAbility>();
                }
            }
            catch (Exception ex)
            {
                abilities = new List<InnateAbility>();
                abilities.Add(new InnateAbility
                {
                    Kin = "Error",
                    Name = "Failed to load",
                    Description = ex.Message,
                    Wp = new JsonElement()
                });
            }

            LoadFavorites();
        }

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

        protected void ToggleFavoriteInnate(string? name) => ToggleFavorite("innate", name);
        protected bool IsFavoriteInnate(string? name) => IsFavorite("innate", name);

        protected int CountFavoritesForKind(string kind)
        {
            if (favorites == null || favorites.Count == 0) return 0;
            return favorites.Count(f => f != null && f.StartsWith($"{kind}|", StringComparison.OrdinalIgnoreCase));
        }
    }

    public class KinEntry
    {
        [JsonPropertyName("kin")]
        public string? Kin { get; set; }
        [JsonPropertyName("abilities")]
        public List<InnateAbilitySource>? Abilities { get; set; }
    }

    public class InnateAbilitySource
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        [JsonPropertyName("wp")]
        public JsonElement Wp { get; set; }
        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }

    public class InnateAbility
    {
        public string? Kin { get; set; }
        public string? Name { get; set; }
        public JsonElement Wp { get; set; }
        public string? Description { get; set; }

        public string WPDisplay
        {
            get
            {
                try
                {
                    if (Wp.ValueKind == JsonValueKind.Undefined || Wp.ValueKind == JsonValueKind.Null)
                        return "-";
                    if (Wp.ValueKind == JsonValueKind.Number && Wp.TryGetInt32(out var n))
                        return n.ToString();
                    if (Wp.ValueKind == JsonValueKind.String)
                        return Wp.GetString() ?? "-";
                }
                catch { }
                return Wp.ToString() ?? "-";
            }
        }
    }
}

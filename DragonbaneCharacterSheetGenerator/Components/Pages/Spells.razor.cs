using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using DragonbaneCharacterSheetGenerator.Shared;

namespace DragonbaneCharacterSheetGenerator.Components.Pages
{
    public class SpellsBase : ComponentBase
    {
        protected SpellsDoc? doc;
        protected string selectedType = "Tricks";
        protected string selectedSchool = "All";
        protected string searchTerm = string.Empty;
        protected string selectedMaxRank = "Any";

        protected bool showModal;
        protected CardView? modalCard;
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

        protected IEnumerable<string> SplitPrerequisites(string? prereq)
        {
            if (string.IsNullOrWhiteSpace(prereq)) return Enumerable.Empty<string>();
            var norm = prereq.Replace(" or ", ",", StringComparison.OrdinalIgnoreCase).Replace(" and ", ",", StringComparison.OrdinalIgnoreCase);
            return norm.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim());
        }

        protected Spell? FindSpellByName(string name)
        {
            if (doc?.Spells == null) return null;
            string n = name.Trim();
            Spell? FindIn(IEnumerable<Spell>? list) => list?.FirstOrDefault(s => string.Equals(s.Name?.Trim(), n, StringComparison.OrdinalIgnoreCase));

            return FindIn(doc.Spells.GeneralSpells) ?? FindIn(doc.Spells.AnimismSpells) ?? FindIn(doc.Spells.ElementalismSpells) ?? FindIn(doc.Spells.MentalismSpells);
        }

        protected void OpenPrereqBySpell(Spell s)
        {
            var card = new CardView { Title = s.Name, Subtitle = $"Rank: {s.Rank}", Body = s.Effect, IsSpell = true, Spell = s };
            modalCard = card;
            showModal = true;
        }
        protected void ToggleFavoriteSpell(string? name) => ToggleFavorite("spell", name);
        protected bool IsFavoriteSpell(string? name) => IsFavorite("spell", name);
        protected void ToggleFavoriteItem(bool isSpell, string? name) => ToggleFavorite(isSpell ? "spell" : "trick", name);
        protected bool IsFavoriteItem(bool isSpell, string? name) => IsFavorite(isSpell ? "spell" : "trick", name);

        protected int CountFavoritesForKind(string kind)
        {
            if (favorites == null || favorites.Count == 0) return 0;
            return favorites.Count(f => f != null && f.StartsWith($"{kind}|", StringComparison.OrdinalIgnoreCase));
        }

        protected IEnumerable<CardView> FilteredCards
        {
            get
            {
                if (doc == null) return Enumerable.Empty<CardView>();

                var cards = new List<CardView>();

                // parse selected max rank once
                int? maxRank = int.TryParse(selectedMaxRank, out var tmp) ? tmp : (int?)null;

                if (selectedType == "Tricks")
                {
                    var all = doc.Tricks?.GeneralTricks ?? new List<Trick>();
                    foreach (var t in all)
                    {
                        if (!MatchesSearch(t.Name, t.Effect)) continue;
                        var card = new CardView { Title = t.Name, Subtitle = $"WP: {t.WpCost}", Body = t.Effect };
                        if (showFavoritesOnly && !IsFavorite("trick", card.Title)) continue;
                        cards.Add(card);
                    }

                    if (selectedSchool == "All")
                    {
                        var tricksBySchool = new Dictionary<string, List<Trick>?> {
                            { "Animism", doc.Tricks?.Animism },
                            { "Elementalism", doc.Tricks?.Elementalism },
                            { "Mentalism", doc.Tricks?.Mentalism }
                        };

                        foreach (var kv in tricksBySchool)
                        {
                            if (kv.Value == null) continue;
                            foreach (var t in kv.Value)
                            {
                                if (!MatchesSearch(t.Name, t.Effect)) continue;
                                var card = new CardView { Title = t.Name, Subtitle = $"WP: {t.WpCost} � {kv.Key}", Body = t.Effect };
                                if (showFavoritesOnly && !IsFavorite("trick", card.Title)) continue;
                                cards.Add(card);
                            }
                        }
                    }
                    else
                    {
                        List<Trick>? list = selectedSchool switch
                        {
                            "Animism" => doc.Tricks?.Animism,
                            "Elementalism" => doc.Tricks?.Elementalism,
                            "Mentalism" => doc.Tricks?.Mentalism,
                            _ => null
                        };
                        if (list != null)
                        {
                            foreach (var t in list)
                            {
                                if (!MatchesSearch(t.Name, t.Effect)) continue;
                                var card = new CardView { Title = t.Name, Subtitle = $"WP: {t.WpCost} � {selectedSchool}", Body = t.Effect };
                                if (showFavoritesOnly && !IsFavorite("trick", card.Title)) continue;
                                cards.Add(card);
                            }
                        }
                    }
                }
                else // Spells
                {
                    foreach (var s in doc.Spells?.GeneralSpells ?? new List<Spell>())
                    {
                        if (!MatchesSearch(s.Name, s.Effect)) continue;

                        if (maxRank.HasValue && int.TryParse(s.Rank?.Trim(), out var rVal) && rVal > maxRank.Value) continue;

                        var card = new CardView { Title = s.Name, Subtitle = $"Rank: {s.Rank} � {ExpandRequirements(s.Requirements)}", Body = s.Effect, IsSpell = true, Spell = s };
                        if (!showFavoritesOnly || IsFavorite("spell", card.Title)) cards.Add(card);
                    }

                    if (selectedSchool == "All")
                    {
                        foreach (var s in doc.Spells?.AnimismSpells ?? new List<Spell>())
                        {
                            if (!MatchesSearch(s.Name, s.Effect)) continue;
                            if (maxRank.HasValue && int.TryParse(s.Rank?.Trim(), out var rVal) && rVal > maxRank.Value) continue;
                            var card = new CardView { Title = s.Name, Subtitle = $"Rank: {s.Rank} � Animism � {ExpandRequirements(s.Requirements)}", Body = s.Effect, IsSpell = true, Spell = s };
                            if (!showFavoritesOnly || IsFavorite("spell", card.Title)) cards.Add(card);
                        }
                        foreach (var s in doc.Spells?.ElementalismSpells ?? new List<Spell>())
                        {
                            if (!MatchesSearch(s.Name, s.Effect)) continue;
                            if (maxRank.HasValue && int.TryParse(s.Rank?.Trim(), out var rVal) && rVal > maxRank.Value) continue;
                            var card = new CardView { Title = s.Name, Subtitle = $"Rank: {s.Rank} � Elementalism � {ExpandRequirements(s.Requirements)}", Body = s.Effect, IsSpell = true, Spell = s };
                            if (!showFavoritesOnly || IsFavorite("spell", card.Title)) cards.Add(card);
                        }
                        foreach (var s in doc.Spells?.MentalismSpells ?? new List<Spell>())
                        {
                            if (!MatchesSearch(s.Name, s.Effect)) continue;
                            if (maxRank.HasValue && int.TryParse(s.Rank?.Trim(), out var rVal) && rVal > maxRank.Value) continue;
                            var card = new CardView { Title = s.Name, Subtitle = $"Rank: {s.Rank} � Mentalism � {ExpandRequirements(s.Requirements)}", Body = s.Effect, IsSpell = true, Spell = s };
                            if (!showFavoritesOnly || IsFavorite("spell", card.Title)) cards.Add(card);
                        }
                    }
                    else if (selectedSchool == "Animism")
                    {
                        foreach (var s in doc.Spells?.AnimismSpells ?? new List<Spell>())
                        {
                            if (!MatchesSearch(s.Name, s.Effect)) continue;
                            if (maxRank.HasValue && int.TryParse(s.Rank?.Trim(), out var rVal) && rVal > maxRank.Value) continue;
                            var card = new CardView { Title = s.Name, Subtitle = $"Rank: {s.Rank} � Animism � {ExpandRequirements(s.Requirements)}", Body = s.Effect, IsSpell = true, Spell = s };
                            if (!showFavoritesOnly || IsFavorite("spell", card.Title)) cards.Add(card);
                        }
                    }
                    else if (selectedSchool == "Elementalism")
                    {
                        foreach (var s in doc.Spells?.ElementalismSpells ?? new List<Spell>())
                        {
                            if (!MatchesSearch(s.Name, s.Effect)) continue;
                            if (maxRank.HasValue && int.TryParse(s.Rank?.Trim(), out var rVal) && rVal > maxRank.Value) continue;
                            var card = new CardView { Title = s.Name, Subtitle = $"Rank: {s.Rank} � Elementalism � {ExpandRequirements(s.Requirements)}", Body = s.Effect, IsSpell = true, Spell = s };
                            if (!showFavoritesOnly || IsFavorite("spell", card.Title)) cards.Add(card);
                        }
                    }
                    else if (selectedSchool == "Mentalism")
                    {
                        foreach (var s in doc.Spells?.MentalismSpells ?? new List<Spell>())
                        {
                            if (!MatchesSearch(s.Name, s.Effect)) continue;
                            if (maxRank.HasValue && int.TryParse(s.Rank?.Trim(), out var rVal) && rVal > maxRank.Value) continue;
                            var card = new CardView { Title = s.Name, Subtitle = $"Rank: {s.Rank} � Mentalism � {ExpandRequirements(s.Requirements)}", Body = s.Effect, IsSpell = true, Spell = s };
                            if (!showFavoritesOnly || IsFavorite("spell", card.Title)) cards.Add(card);
                        }
                    }
                }

                return cards;
            }
        }

        protected bool MatchesSearch(string? name, string? body)
        {
            if (string.IsNullOrWhiteSpace(searchTerm)) return true;
            return (name ?? string.Empty).Contains(searchTerm, StringComparison.OrdinalIgnoreCase) || (body ?? string.Empty).Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
        }

        protected void OpenModal(CardView card, bool isSpell)
        {
            modalCard = card;
            showModal = true;
            modalCard.IsSpell = isSpell;
        }

        protected void CloseModal()
        {
            showModal = false;
            modalCard = null;
        }

        protected string GetScaledEffect(CardView card, int powerLevel = 1)
        {
            var text = card.Body ?? string.Empty;
            try
            {
                if (powerLevel <= 1) return text;

                var lower = text.ToLowerInvariant();
                var increasesNumberOfDice = lower.Contains("increases the number of dice") || lower.Contains("number of dice rolled") || lower.Contains("each additional power level increases the number of dice") || lower.Contains("each power level beyond the first increases the number of dice");
                var increasesDamageByDie = Regex.Match(lower, "increases the damage by d(\\d+)");

                var m = Regex.Match(text, "(\\d+)D(\\d+)", RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    int baseCount = int.Parse(m.Groups[1].Value);
                    int dieSize = int.Parse(m.Groups[2].Value);

                    if (increasesNumberOfDice)
                    {
                        int newCount = baseCount + (powerLevel - 1);
                        return Regex.Replace(text, "(\\d+)D(\\d+)", $"{newCount}D{dieSize}");
                    }

                    if (increasesDamageByDie.Success)
                    {
                        int incDie = int.Parse(increasesDamageByDie.Groups[1].Value);
                        return text + $"\n\nScaled damage: {baseCount}D{dieSize} + {powerLevel - 1}D{incDie} (for power level {powerLevel})";
                    }
                }

                if (increasesDamageByDie.Success)
                {
                    int incDie = int.Parse(increasesDamageByDie.Groups[1].Value);
                    return text + $"\n\nScaled increase: +{powerLevel - 1}D{incDie} (for power level {powerLevel})";
                }

                return text;
            }
            catch
            {
                return text;
            }
        }

        protected static string ExpandRequirements(string? req)
        {
            if (string.IsNullOrWhiteSpace(req)) return string.Empty;
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "W", "Word (spoken)" },
                { "G", "Gesture" },
                { "I", "Ingredient" },
                { "F", "Focus (holy symbol)" }
            };

            var parts = req.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var expanded = new List<string>();
            foreach (var p in parts)
            {
                if (map.TryGetValue(p, out var val)) expanded.Add(val);
                else expanded.Add(p);
            }

            return string.Join(", ", expanded);
        }

        protected class CardView
        {
            public string? Title { get; set; }
            public string? Subtitle { get; set; }
            public string? Body { get; set; }
            public bool IsSpell { get; set; }
            public Spell? Spell { get; set; }
        }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var overridePath = Path.Combine(FileSystem.AppDataDirectory, "doc", "spells.json");
                if (File.Exists(overridePath))
                {
                    await using var stream = File.OpenRead(overridePath);
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    doc = await JsonSerializer.DeserializeAsync<SpellsDoc>(stream, options);
                }
                else
                {
                    await using var stream = await FileSystem.OpenAppPackageFileAsync("wwwroot/doc/spells.json");
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    doc = await JsonSerializer.DeserializeAsync<SpellsDoc>(stream, options);
                }
            }
            catch (Exception ex)
            {
                doc = new SpellsDoc();
                doc.Spells = new SpellsSection { GeneralSpells = new List<Spell> { new Spell { Name = "Error loading spells", Rank = "-", Requirements = "-", Range = "-", Duration = "-", Effect = ex.Message } } };
            }

            LoadFavorites();
        }
    }
}

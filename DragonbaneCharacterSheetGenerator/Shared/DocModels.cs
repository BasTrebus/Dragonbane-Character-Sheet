using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DragonbaneCharacterSheetGenerator.Shared
{
    public class SpellsDoc
    {
        [JsonPropertyName("tricks")]
        public TricksSection? Tricks { get; set; }

        [JsonPropertyName("spells")]
        public SpellsSection? Spells { get; set; }

        [JsonPropertyName("notes")]
        public string? Notes { get; set; }

        [JsonPropertyName("version")]
        public string? Version { get; set; }
    }

    public class TricksSection
    {
        [JsonPropertyName("general_tricks")]
        public List<Trick>? GeneralTricks { get; set; }

        [JsonPropertyName("Animism")]
        public List<Trick>? Animism { get; set; }

        [JsonPropertyName("Elementalism")]
        public List<Trick>? Elementalism { get; set; }

        [JsonPropertyName("Mentalism")]
        public List<Trick>? Mentalism { get; set; }
    }

    public class SpellsSection
    {
        [JsonPropertyName("general_spells")]
        public List<Spell>? GeneralSpells { get; set; }

        [JsonPropertyName("animism_spells")]
        public List<Spell>? AnimismSpells { get; set; }

        [JsonPropertyName("elementalism_spells")]
        public List<Spell>? ElementalismSpells { get; set; }

        [JsonPropertyName("mentalism_spells")]
        public List<Spell>? MentalismSpells { get; set; }
    }

    public class Trick
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("wp_cost")]
        public int? WpCost { get; set; }

        [JsonPropertyName("effect")]
        public string? Effect { get; set; }
    }

    public class Spell
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("rank")]
        public string? Rank { get; set; }

        [JsonPropertyName("requirements")]
        public string? Requirements { get; set; }

        [JsonPropertyName("cast_time")]
        public string? CastTime { get; set; }

        [JsonPropertyName("range")]
        public string? Range { get; set; }

        [JsonPropertyName("duration")]
        public string? Duration { get; set; }

        [JsonPropertyName("effect")]
        public string? Effect { get; set; }

        [JsonPropertyName("prerequisite")]
        public string? Prerequisite { get; set; }
    }

    public class HeroicAbilitiesDoc
    {
        [JsonPropertyName("abilities")]
        public List<HeroicAbility>? Abilities { get; set; }

        [JsonPropertyName("footer_note")]
        public string? FooterNote { get; set; }
    }

    public class HeroicAbility
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("wp")]
        public JsonElement Wp { get; set; }

        [JsonPropertyName("skill")]
        public string? Skill { get; set; }

        [JsonPropertyName("skill_min")]
        public int? SkillMin { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        public string WPDisplay
        {
            get
            {
                if (Wp.ValueKind == JsonValueKind.Undefined || Wp.ValueKind == JsonValueKind.Null)
                    return "-";
                if (Wp.ValueKind == JsonValueKind.Number && Wp.TryGetInt32(out var n))
                    return n.ToString();
                if (Wp.ValueKind == JsonValueKind.String)
                    return Wp.GetString() ?? "-";
                return Wp.ToString();
            }
        }

        public string SkillMinDisplay => SkillMin?.ToString() ?? "-";
    }
}

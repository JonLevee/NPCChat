using System.Collections.Generic;

namespace NPChat.CharacterClasses
{
    public class Trait
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Dictionary<string, int> Weights { get; set; } = new Dictionary<string, int>();
    }

    public class TraitModifier
    {
        public string TraitName { get; set; } = string.Empty;
        public int Modifier { get; set; } = 0;
    }

    public class CharacterArchetype
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<TraitModifier> TraitModifiers { get; set; } = new List<TraitModifier>();
    }

    public class CharacterCoreMetadata
    {
        public Dictionary<string, Trait> Traits { get; set; } = new Dictionary<string, Trait>();
        public Dictionary<string, CharacterArchetype> Archetype { get; set; } = new Dictionary<string, CharacterArchetype>();
    }

    public class CharacterCoreMetadataProvider
    {
        public CharacterCoreMetadata GetCharacterCoreMetadata()
        {
            return new CharacterCoreMetadata();
        }

    }
}
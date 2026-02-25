using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace NPChat.CharacterClasses
{
    public enum TraitName
    {
        Warmth,
        Confidence,
        Gossip,
        Magical
    }

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

    public class CharacterPersonality
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [YamlMember(Alias = "foo", )]
        public List<TraitModifier> TraitModifiers { get; set; } = new List<TraitModifier>();
    }

    public class CharacterCoreMetadata
    {
        public List<string> Traits { get; set; } = new List<string>();
        public Dictionary<string, CharacterPersonality> Archetypes { get; set; } = new Dictionary<string, CharacterPersonality>();
    }

    public class CharacterCoreMetadataProvider
    {
        public CharacterCoreMetadata GetCharacterCoreMetadata()
        {
            var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "CoreMetadata.yaml", SearchOption.AllDirectories);
            var file = files.First();
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .Build();
            var instance = deserializer.Deserialize<CharacterCoreMetadata>(File.ReadAllText(file));
            return instance;
        }

    }
}
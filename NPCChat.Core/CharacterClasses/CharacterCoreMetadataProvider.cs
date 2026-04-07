using Microsoft.Extensions.DependencyInjection;
using NPCChat.Core.Attributes;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace NPChat.CharacterClasses
{
    [Singleton]
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
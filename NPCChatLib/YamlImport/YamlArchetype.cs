using System.ComponentModel.DataAnnotations;
using YamlDotNet.Serialization;

namespace NPCChatLib.YamlImport
{
    public class YamlArchetype
    {
        [Required]
        [YamlMember]
        public string Name { get; }

        public YamlArchetype(string name) 
        {
            Name = name;
        }

    }
}

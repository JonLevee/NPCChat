using System.ComponentModel.DataAnnotations;
using System.Drawing;
using YamlDotNet.Serialization;

namespace NPCChatLib.YamlImport
{
    public class YamlCharacter : YamlObject
    {
    }

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

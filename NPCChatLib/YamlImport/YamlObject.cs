using System.ComponentModel.DataAnnotations;
using System.Drawing;
using YamlDotNet.Serialization;

namespace NPCChatLib.YamlImport
{
    public interface IYamlObject
    {
        [Required]
        [YamlMember]
        public string Name { get; set; };

        [Required]
        [YamlMember]
        public Rectangle Location { get; set; }
    }

    public abstract class YamlObjectBase : IYamlObject
    {
        [Required]
        [YamlMember]
        public string Name { get; set; }
        [Required]
        [YamlMember]
        public Rectangle Location { get; set; }
    }
}

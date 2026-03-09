using System.ComponentModel.DataAnnotations;
using System.Drawing;
using YamlDotNet.Serialization;

namespace NPCChatLib.YamlImport
{
    public interface IYamlObject
    {
        string Name { get; set; }
        Point Location { get; set; }
        Size Size { get; set; }
        string Description => $"Name={Name}, Location={Location}, Size={Size}";
    }

    public abstract class YamlObjectBase : IYamlObject
    {
        [Required]
        [YamlMember]
        public string Name { get; set; }
        
        [Required]
        [YamlMember]
        public Point Location { get; set; }

        [Required]
        [YamlMember]
        public Size Size { get; set; }
    }
}

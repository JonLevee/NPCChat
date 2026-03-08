using System.ComponentModel.DataAnnotations;
using System.Drawing;
using YamlDotNet.Serialization;

namespace NPCChatLib.YamlImport
{
    public class YamlObject
    {
        [Required]
        [YamlMember]
        public string Name { get; set; } = string.Empty;

        [Required]
        [YamlMember]
        public Point Location { get; set; } = new Point();

        [Required]
        [YamlMember]
        public Size Size { get; set; } = new Size();

    }
}

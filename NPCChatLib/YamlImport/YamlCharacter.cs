using System.ComponentModel.DataAnnotations;
using System.Drawing;
using YamlDotNet.Serialization;

namespace NPCChatLib.YamlImport
{
    public class YamlCharacter : YamlImportable
    {
        [Required]
        [YamlMember]
        public string Name { get; set; } = string.Empty;
        public Point Location { get; set; } = new Point();
        public Size Size { get; set; } = new Size();
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace NPCChatLib.YamlImport
{
    public abstract class YamlImportable : IDisposable
    {
        public void Dispose()
        {
        }

    }
    public class YamlImportMoodAxes : YamlImportable
    {
        [YamlMember]
        public Dictionary<string, List<string>> Dispositions { get; set; } = new Dictionary<string, List<string>>();
    }

    public class YamlBuilding : YamlImportable
    {
        [Required]
        [YamlMember]
        public string Name { get; set; } = string.Empty;
        public Point Location { get; set; } = new Point();
        public Size Size { get; set; } = new Size();
    }

    public class WorldYaml
    {
        public Size WorldSize { get; set; } = new Size(10, 10);
        public List<YamlBuilding> Buildings { get; set; } = new List<YamlBuilding>();
        public WorldYaml()
        {
        }
    }
}

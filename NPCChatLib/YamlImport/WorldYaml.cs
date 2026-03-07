using System.Collections.Generic;
using System.Drawing;

namespace NPCChatLib.YamlImport
{
    public class WorldYaml
    {
        public Size WorldSize { get; set; } = new Size(10, 10);
        public List<YamlBuilding> Buildings { get; set; } = new List<YamlBuilding>();
        public WorldYaml()
        {
        }
    }
}

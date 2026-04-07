using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace NPCChat.Core.YamlImport
{
    public class YamlImportMoodAxes
    {
        [YamlMember]
        public Dictionary<string, List<string>> Dispositions { get; set; } = new Dictionary<string, List<string>>();
    }
}

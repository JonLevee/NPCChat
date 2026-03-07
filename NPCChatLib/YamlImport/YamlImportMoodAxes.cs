using System;
using System.Collections.Generic;
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
}

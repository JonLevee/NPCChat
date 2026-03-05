using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace NPCChatLib.YamlImport
{
    internal abstract class YamlImportable : IDisposable
    {
        public void Dispose()
        {
        }

    }
    internal class YamlImportMoodAxes : YamlImportable
    {
        [YamlMember]
        public Dictionary<string, List<string>> Dispositions { get; set; } = new Dictionary<string, List<string>>();
    }
}

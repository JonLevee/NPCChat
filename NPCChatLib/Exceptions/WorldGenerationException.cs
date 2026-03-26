using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPCChatLib.Extensions;
using NPCChatLib.WorldClasses;

namespace NPCChatLib.Exceptions
{
    public class WorldGenerationException : Exception
    {
        public WorldGenerationException(string message) : base(message)
        {
        }
        public WorldGenerationException(WorldObject o, List<(string name, WorldObject o)> conflicts) : base(GetErrorText(o, conflicts))
        {
        }

        private static string GetErrorText(WorldObject o, List<(string name, WorldObject o)> conflicts)
        {
            var errorText = new StringBuilder($"new object {o.GetDescription()} conflicts with:\r\n");
            foreach (var conflict in conflicts)
            {
                errorText.AppendLine($"{conflict.name}: {conflict.o.GetDescription()}");
            }
            return errorText.ToString();
        }
    }
}

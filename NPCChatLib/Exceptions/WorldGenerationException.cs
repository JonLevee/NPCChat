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
        public WorldGenerationException(WorldObject o, List<WorldObject> conflicts) : base(GetErrorText(o, conflicts))
        {
        }

        private static string GetErrorText(WorldObject o, List<WorldObject> conflicts)
        {
            var errorText = new StringBuilder($"new object {o.GetDescription()} conflicts with:\r\n");
            foreach (var conflict in conflicts)
            {
                if (conflict == o)
                    errorText.AppendLine($"  [conflicting id]: {conflict.GetDescription()}");
                else
                    errorText.AppendLine($"  [conflicting position]: {conflict.GetDescription()}");
            }
            return errorText.ToString();
        }
    }
}

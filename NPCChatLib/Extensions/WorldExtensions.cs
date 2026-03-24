using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NPCChatLib.WorldClasses;

namespace NPCChatLib.Extensions
{
    public static class WorldExtensions
    {
        public static bool IsStatic(this WorldObject wObject) => wObject.Type < WorldObjectType.Player;
        public static bool IsDynamic(this WorldObject wObject) => wObject.Type >= WorldObjectType.Player;

        public static string GetAutoDebugDisplayText(this IAutoDebugDisplay instance)
        {
            var text = string.Join(", ", GetDebugDisplayItems(instance));
            return text;
        }

        private static IEnumerable<string> GetDebugDisplayItems(IAutoDebugDisplay instance)
        {
            var members = instance
                .GetType()
                .GetMembers(BindingFlags.Public | BindingFlags.Public | BindingFlags.Instance)
                .Where(member => member.GetCustomAttribute<DebugDisplayAttribute>() != null);
            foreach (var member in members)
            {
                var value = member is PropertyInfo propertyInfo
                    ? propertyInfo.GetValue(instance)
                    : member is FieldInfo fieldInfo
                        ? fieldInfo.GetValue(instance)
                        : throw new InvalidOperationException($"Member [{member.Name}] is a {member.MemberType} but should be either Field or Property");
                var text = value is IAutoDebugDisplay autoDisplay ? GetAutoDebugDisplayText(autoDisplay) : value.ToString();
                if (text.Contains(' ') || text == string.Empty)
                    text = "[" + text + "]";
                yield return $"{member.Name}: {text}";
            }
        }
    }
}

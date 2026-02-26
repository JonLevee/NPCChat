using Microsoft.Extensions.DependencyInjection;
using NPCChatLib.Attributes;
using System.Collections.Generic;

namespace NPChat.CharacterClasses
{
    [Singleton]
    public class CharacterArchetypes : Dictionary<string, CharacterArchetype>
    {
    }

    [Transient]
    public class CharacterArchetype
    {

    }

}
using Microsoft.Extensions.DependencyInjection;
using NPCChat.Core.Attributes;
using System.Collections.Generic;

namespace NPCChat.Core.CharacterClasses
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
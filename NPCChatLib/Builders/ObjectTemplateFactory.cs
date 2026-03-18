using System;
using NPCChatLib.Attributes;

namespace NPCChatLib.Builders
{
    public enum TownTemplateName
    {
        StandardB3H3C5,
    }

    public enum BuildingTemplateName
    {
        Standard5x7
    }

    [Singleton]
    public class ObjectTemplateFactory
    {
        private readonly ObjectTemplate[] _templates =
            [
                new BuildingTemplate(
                    BuildingTemplateName.Standard5x7
                    )
            ];
        public ObjectTemplateFactory()
        {
        }
    }

    public abstract class ObjectTemplate
    {
        public string Name { get; }

        public ObjectTemplate(Enum name)
        {
            Name = name.ToString();
        }
    }

    public class BuildingTemplate : ObjectTemplate
    {
        public BuildingTemplate(Enum name) : base(name)
        {
        }
    }
}

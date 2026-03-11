using NPCChatLib.Attributes;

namespace NPCChatLib.Builders
{
    public enum TownTemplateName
    {
        StandardB3H3C5,
    }

    [Singleton]
    public class TownTemplateFactory
    {
        private readonly TownTemplate[] _templates =
            [
                new TownTemplate(
                    TownTemplateName.StandardB3H3C5,
                    )
            ];
        public TownTemplateFactory()
        {
        }
    }

    public class TownTemplate
    {
        public TownTemplateName Name { get; }

        public TownTemplate(TownTemplateName name)
        {
            Name = name;
        }
    }
}

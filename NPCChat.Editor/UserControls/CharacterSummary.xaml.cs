using System.Windows.Controls;
using NPCChatLib.WorldClasses;
using UserControl = System.Windows.Controls.UserControl;

namespace NPCChat.Editor.UserControls
{
    public partial class CharacterSummary : UserControl
    {
        public WorldObject WorldObject { get; }

        public CharacterSummary(WorldObject worldObject)
        {
            WorldObject = worldObject;
            InitializeComponent();
            DataContext = worldObject;
        }
    }
}

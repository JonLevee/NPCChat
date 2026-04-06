using System.Windows.Controls;
using System.Windows.Media;
using NPCChatLib.WorldClasses;
using Color = System.Windows.Media.Color;
using UserControl = System.Windows.Controls.UserControl;

namespace NPCChat.Editor.UserControls
{
    public partial class CharacterSummary : UserControl
    {
        private static readonly SolidColorBrush NormalBorder = new(Color.FromRgb(51, 65, 85));   // #334155
        private static readonly SolidColorBrush HighlightBorder = new(Color.FromRgb(96, 165, 250)); // #60A5FA

        public WorldObject WorldObject { get; }

        public CharacterSummary(WorldObject worldObject)
        {
            WorldObject = worldObject;
            InitializeComponent();
            DataContext = worldObject;
        }

        public bool IsSelected { get; private set; }

        public void SetSelected(bool selected)
        {
            IsSelected = selected;
            SummaryBorder.BorderBrush = selected ? HighlightBorder : NormalBorder;
        }

        public void SetHighlighted(bool highlighted)
        {
            SummaryBorder.BorderBrush = highlighted ? HighlightBorder : NormalBorder;
        }
    }
}

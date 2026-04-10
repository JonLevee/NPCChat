using System.Windows;
using System.Windows.Controls;
using UserControl = System.Windows.Controls.UserControl;

namespace NPCChat.Editor.UserControls
{
    public partial class InventoryPanel : UserControl
    {
        public InventoryPanel()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Refreshes the panel with the current inventory snapshot and makes it visible.
        /// </summary>
        /// <param name="slots">Name/Quantity pairs as returned by WorldData.SnapshotInventory.</param>
        public void Refresh((string Name, int Quantity)[] slots)
        {
            if (slots.Length == 0)
            {
                SlotsList.ItemsSource = null;
                EmptyText.Visibility  = Visibility.Visible;
            }
            else
            {
                SlotsList.ItemsSource = slots.Select(s => new { s.Name, s.Quantity }).ToList();
                EmptyText.Visibility  = Visibility.Collapsed;
            }

            Visibility = Visibility.Visible;
        }

        public void Hide()
        {
            Visibility = Visibility.Collapsed;
        }
    }
}

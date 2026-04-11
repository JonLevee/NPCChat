using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using NPCChat.Core.ShopClasses;
using UserControl = System.Windows.Controls.UserControl;

namespace NPCChat.Editor.UserControls
{
    public partial class ShopPanel : UserControl
    {
        // ── View-model records ──────────────────────────────────────────────

        private sealed record BuyRow(string ItemId, string Name, int BuyPrice)
        {
            public string PriceText => $"{BuyPrice}g";
        }

        private sealed record SellRow(string ItemId, string Name, int Quantity, int SellPrice)
        {
            public string QtyText      => $"x{Quantity}";
            public string SellPriceText => $"{SellPrice}g";
        }

        // ── State ───────────────────────────────────────────────────────────

        private List<BuyRow>  _buyRows  = [];
        private List<SellRow> _sellRows = [];

        // ── Events (consumed by MainWindow) ─────────────────────────────────

        /// <summary>Fired when the player clicks "Buy 1". Arg is the item id.</summary>
        public event Action<string>? BuyOne;

        /// <summary>Fired when the player clicks "Sell 1". Arg is the item id.</summary>
        public event Action<string>? SellOne;

        public ShopPanel()
        {
            InitializeComponent();
        }

        // ── Public API ───────────────────────────────────────────────────────

        /// <summary>
        /// Refreshes and shows the shop panel.
        /// </summary>
        /// <param name="shopName">NPC display name (e.g. "Blacksmith").</param>
        /// <param name="stock">Items the NPC sells, from WorldData.SnapshotShopStock.</param>
        /// <param name="playerItems">Player inventory, from WorldData.SnapshotInventoryDetailed.</param>
        /// <param name="sellMultiplier">Shop's sell-back rate (0–1), from ShopComponent.</param>
        /// <param name="playerGold">Player's current gold count.</param>
        public void Refresh(
            string shopName,
            ShopEntry[] stock,
            (string ItemId, string Name, int Quantity, int BaseValue)[] playerItems,
            float sellMultiplier,
            int playerGold)
        {
            ShopTitle.Text = $"{shopName}'s Shop";
            GoldLabel.Text = $"Gold: {playerGold}";

            _buyRows = stock
                .Select(e => new BuyRow(e.ItemId, e.ItemDef.Name, e.BuyPrice))
                .ToList();

            _sellRows = playerItems
                .Where(i => i.ItemId != "gold_coin")   // don't offer to sell currency
                .Select(i => new SellRow(
                    i.ItemId, i.Name, i.Quantity,
                    Math.Max(1, (int)Math.Floor(i.BaseValue * sellMultiplier))))
                .ToList();

            BuyList.ItemsSource  = _buyRows;
            SellList.ItemsSource = _sellRows;

            Visibility = Visibility.Visible;
        }

        public void Hide()
        {
            Visibility = Visibility.Collapsed;
        }

        // ── Tab switching ────────────────────────────────────────────────────

        private void BuyTab_Click(object sender, RoutedEventArgs e)
        {
            BuyPanel.Visibility  = Visibility.Visible;
            SellPanel.Visibility = Visibility.Collapsed;
        }

        private void SellTab_Click(object sender, RoutedEventArgs e)
        {
            BuyPanel.Visibility  = Visibility.Collapsed;
            SellPanel.Visibility = Visibility.Visible;
        }

        // ── Transaction buttons ──────────────────────────────────────────────

        private void BuyButton_Click(object sender, RoutedEventArgs e)
        {
            if (BuyList.SelectedItem is BuyRow row)
                BuyOne?.Invoke(row.ItemId);
        }

        private void SellButton_Click(object sender, RoutedEventArgs e)
        {
            if (SellList.SelectedItem is SellRow row)
                SellOne?.Invoke(row.ItemId);
        }
    }
}

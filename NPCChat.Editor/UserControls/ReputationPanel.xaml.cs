using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using NPCChat.Core.FactionClasses;
using NPCChat.Core.LoadingProviderClasses;
using UserControl = System.Windows.Controls.UserControl;

namespace NPCChat.Editor.UserControls
{
    public partial class ReputationPanel : UserControl
    {
        public ReputationPanel()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Refreshes the panel with the current reputation snapshot and makes it visible.
        /// </summary>
        public void Refresh(ReputationLog? repLog, StaticData staticData)
        {
            if (repLog is null || staticData.Factions.Count == 0)
            {
                FactionList.ItemsSource = null;
                Visibility = Visibility.Visible;
                return;
            }

            var items = new List<object>();
            foreach (var faction in staticData.Factions.Values)
            {
                int score = repLog.GetReputation(faction.Id);
                var tier  = repLog.GetTier(faction);
                items.Add(new
                {
                    FactionName = faction.Name,
                    Score       = score >= 0 ? $"+{score}" : score.ToString(),
                    Tier        = tier.ToString(),
                    TierColor   = TierColor(tier)
                });
            }

            FactionList.ItemsSource = items;
            Visibility = Visibility.Visible;
        }

        public void Hide() => Visibility = Visibility.Collapsed;

        private static string TierColor(ReputationTier tier) => tier switch
        {
            ReputationTier.Exalted    => "#FBBF24",
            ReputationTier.Revered    => "#60A5FA",
            ReputationTier.Honored    => "#34D399",
            ReputationTier.Friendly   => "#86EFAC",
            ReputationTier.Neutral    => "#94A3B8",
            ReputationTier.Unfriendly => "#F97316",
            _                         => "#F87171"   // Hostile
        };
    }
}

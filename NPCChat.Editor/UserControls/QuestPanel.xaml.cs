using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using NPCChat.Core.QuestClasses;
using NPCChat.Core.WorldClasses;
using UserControl = System.Windows.Controls.UserControl;

namespace NPCChat.Editor.UserControls
{
    public partial class QuestPanel : UserControl
    {
        public QuestPanel()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Refreshes the panel with current quest state. Makes the panel visible.
        /// </summary>
        /// <param name="questLog">The player's quest log (may be null).</param>
        /// <param name="getItemCount">Thread-safe inventory count delegate.</param>
        public void Refresh(QuestLog? questLog, System.Func<string, int>? getItemCount)
        {
            if (questLog is null || questLog.ActiveQuests.Count == 0)
            {
                QuestList.ItemsSource = null;
                EmptyText.Visibility  = Visibility.Visible;
                Visibility = Visibility.Visible;
                return;
            }

            EmptyText.Visibility = Visibility.Collapsed;

            var items = new List<object>();
            foreach (var record in questLog.ActiveQuests)
            {
                var objectives = new List<object>();
                foreach (var obj in record.Def.Objectives)
                {
                    int current = 0;
                    if (obj.Kind == QuestObjectiveKind.CollectItem ||
                        obj.Kind == QuestObjectiveKind.DeliverItem)
                    {
                        current = getItemCount?.Invoke(obj.TargetId) ?? 0;
                    }
                    bool done = current >= obj.RequiredCount;
                    objectives.Add(new
                    {
                        obj.Description,
                        ProgressText  = $"{current}/{obj.RequiredCount}",
                        ProgressColor = done ? "#34D399" : "#94A3B8"
                    });
                }
                items.Add(new { record.Def.Name, Objectives = objectives });
            }

            QuestList.ItemsSource = items;
            Visibility = Visibility.Visible;
        }

        public void Hide()
        {
            Visibility = Visibility.Collapsed;
        }
    }
}

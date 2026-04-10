using System.Windows;
using System.Windows.Controls;
using NPCChat.Core.DialogueClasses;
using UserControl = System.Windows.Controls.UserControl;

namespace NPCChat.Editor.UserControls
{
    public partial class DialoguePanel : UserControl
    {
        public DialoguePanel()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Refreshes the panel to reflect the current session state and makes it visible.
        /// </summary>
        public void Refresh(DialogueSession session)
        {
            NpcNameText.Text = session.NpcName;
            SpeechText.Text  = session.DisplayText;

            switch (session.State)
            {
                case DialogueSessionState.NpcLine:
                    ChoicesList.Visibility = Visibility.Collapsed;
                    HintText.Text = "Space / Enter to continue  ·  Esc to close";
                    break;

                case DialogueSessionState.PlayerChoice:
                    var labels = session.VisibleChoices
                        .Select(c => $"[{c.Index}]  {c.Choice.Label}")
                        .ToList();
                    ChoicesList.ItemsSource = labels;
                    ChoicesList.Visibility  = Visibility.Visible;
                    HintText.Text = $"1–{session.VisibleChoices.Count} to choose  ·  Esc to close";
                    break;

                case DialogueSessionState.Complete:
                    Hide();
                    return;
            }

            Visibility = Visibility.Visible;
        }

        public void Hide()
        {
            Visibility = Visibility.Collapsed;
        }
    }
}

namespace NPCChat.Core.QuestClasses
{
    public enum QuestStatus { Active, Complete }

    /// <summary>
    /// Mutable runtime record tracking a player's progress on one active quest.
    /// Owned and accessed exclusively on the UI thread (created/completed via dialogue effects).
    /// </summary>
    public sealed class QuestRecord
    {
        public QuestDef Def { get; }
        public QuestStatus Status { get; set; } = QuestStatus.Active;

        public QuestRecord(QuestDef def) => Def = def;
    }
}

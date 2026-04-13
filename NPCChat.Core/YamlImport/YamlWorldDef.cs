#nullable enable
using System.Collections.Generic;
using NPCChat.Core.ItemClasses;
using NPCChat.Core.QuestClasses;
using NPCChat.Core.WorldClasses;

namespace NPCChat.Core.YamlImport
{
    // ── Root document ────────────────────────────────────────────────────────────

    /// <summary>Root of a world definition YAML file.</summary>
    public sealed class YamlWorldDef
    {
        public YamlStaticDataDef StaticData { get; set; } = new();
        public YamlObjectsDef    Objects    { get; set; } = new();
    }

    // ── Static data ──────────────────────────────────────────────────────────────

    public sealed class YamlStaticDataDef
    {
        public List<YamlItemDef>    Items    { get; set; } = [];
        public List<YamlFactionDef> Factions { get; set; } = [];
        public List<YamlQuestDef>   Quests   { get; set; } = [];
    }

    public sealed class YamlItemDef
    {
        public string  Id          { get; set; } = "";
        public string  Name        { get; set; } = "";
        public ItemKind Kind       { get; set; }
        public int     MaxStack    { get; set; } = 1;
        public float   Weight      { get; set; }
        public int     BaseValue   { get; set; }
        public string? Description { get; set; }
    }

    public sealed class YamlFactionDef
    {
        public string Id                 { get; set; } = "";
        public string Name               { get; set; } = "";
        public string Description        { get; set; } = "";
        public int    FriendlyThreshold  { get; set; } = 50;
        public int    HostileThreshold   { get; set; } = -25;
    }

    public sealed class YamlQuestDef
    {
        public string                      Id          { get; set; } = "";
        public string                      Name        { get; set; } = "";
        public string                      Description { get; set; } = "";
        public List<YamlQuestObjectiveDef> Objectives  { get; set; } = [];
        public List<YamlQuestRewardDef>    Rewards     { get; set; } = [];
    }

    public sealed class YamlQuestObjectiveDef
    {
        public string             Id            { get; set; } = "";
        public QuestObjectiveKind Kind          { get; set; }
        public string             Description   { get; set; } = "";
        public string             TargetId      { get; set; } = "";
        public int                RequiredCount { get; set; } = 1;
    }

    public sealed class YamlQuestRewardDef
    {
        public string ItemId   { get; set; } = "";
        public int    Quantity { get; set; } = 1;
    }

    // ── World objects ─────────────────────────────────────────────────────────────

    public sealed class YamlObjectsDef
    {
        public YamlPlayerDef?          Player    { get; set; }
        public List<YamlStaticObjDef>  Static    { get; set; } = [];
        public List<YamlNpcDef>        Npcs      { get; set; } = [];
        public List<YamlCarryableDef>  Carryable { get; set; } = [];
    }

    public sealed class YamlPlayerDef
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    /// <summary>A non-moveable world object (Building, Obstacle, etc.).</summary>
    public sealed class YamlStaticObjDef
    {
        public WorldObjectKind Kind { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int W { get; set; }
        public int H { get; set; }
    }

    /// <summary>
    /// An NPC or mob. <see cref="BehaviorType"/> maps to a registered behavior
    /// setup (e.g. "blacksmith", "farmer", "guard").
    /// <see cref="Params"/> carries type-specific string parameters.
    /// </summary>
    public sealed class YamlNpcDef
    {
        public string                      BehaviorType { get; set; } = "";
        public int                         X            { get; set; }
        public int                         Y            { get; set; }
        public Dictionary<string, string>  Params       { get; set; } = new();
    }

    /// <summary>A loose item lying on the map.</summary>
    public sealed class YamlCarryableDef
    {
        public string ItemId   { get; set; } = "";
        public int    X        { get; set; }
        public int    Y        { get; set; }
        public int    Quantity { get; set; } = 1;
    }
}

#nullable enable
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using NPCChat.Core.Attributes;
using NPCChat.Core.Builders;
using NPCChat.Core.FactionClasses;
using NPCChat.Core.ItemClasses;
using NPCChat.Core.QuestClasses;
using NPCChat.Core.WorldBuilderTemplates;
using NPCChat.Core.WorldClasses;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace NPCChat.Core.YamlImport
{
    /// <summary>
    /// Deserializes a world definition YAML file and populates WorldData.
    /// Call <see cref="LoadFromFile"/> or <see cref="LoadFromYaml"/> once per
    /// scoped service lifetime (i.e. once per game/test).
    /// </summary>
    [Scoped]
    public sealed class YamlWorldLoader
    {
        private readonly WorldDataBuilder _builder;

        private static readonly IDeserializer Deserializer =
            new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

        public YamlWorldLoader(WorldDataBuilder builder)
        {
            _builder = builder;
        }

        // ── Public API ───────────────────────────────────────────────────────────

        /// <summary>
        /// Reads a YAML file from disk, resolves any <c>includes:</c> entries relative
        /// to that file's directory, merges all defs, then builds the world.
        /// </summary>
        public WorldData LoadFromFile(string yamlPath)
        {
            var def = LoadAndMerge(Path.GetFullPath(yamlPath));
            Apply(def);
            return _builder.World;
        }

        /// <summary>Builds the world from an already-loaded YAML string (no include resolution).</summary>
        public WorldData LoadFromYaml(string yaml)
        {
            var def = Deserializer.Deserialize<YamlWorldDef>(yaml);
            Apply(def);
            return _builder.World;
        }

        private static YamlWorldDef LoadAndMerge(string fullPath)
        {
            var def = Deserializer.Deserialize<YamlWorldDef>(File.ReadAllText(fullPath));
            if (def.Includes.Count == 0) return def;

            var dir = Path.GetDirectoryName(fullPath)!;
            foreach (var include in def.Includes)
            {
                var included = LoadAndMerge(Path.GetFullPath(Path.Combine(dir, include)));
                Merge(def, included);
            }
            return def;
        }

        private static void Merge(YamlWorldDef target, YamlWorldDef source)
        {
            target.StaticData.Items.AddRange(source.StaticData.Items);
            target.StaticData.Factions.AddRange(source.StaticData.Factions);
            target.StaticData.Quests.AddRange(source.StaticData.Quests);
            target.Objects.Static.AddRange(source.Objects.Static);
            target.Objects.Npcs.AddRange(source.Objects.Npcs);
            target.Objects.Carryable.AddRange(source.Objects.Carryable);
            if (target.Objects.Player is null && source.Objects.Player is not null)
                target.Objects.Player = source.Objects.Player;
        }

        // ── Private helpers ──────────────────────────────────────────────────────

        private void Apply(YamlWorldDef def)
        {
            RegisterStaticData(def.StaticData);

            // Static world objects created directly — no template dependency needed.
            foreach (var obj in def.Objects.Static)
                AddStaticObject(obj);

            // Carryable items created directly from the item catalog.
            foreach (var c in def.Objects.Carryable)
                AddCarryable(c);

            // Player and NPCs go through Templates so behavior/component setup
            // stays in one place.
            using var t = _builder.GetTemplates();

            if (def.Objects.Player is { } player)
                t.AddPlayer(player.X, player.Y);

            foreach (var npc in def.Objects.Npcs)
                ApplyNpc(t, npc);
        }

        // ── Static data registration ─────────────────────────────────────────────

        private void RegisterStaticData(YamlStaticDataDef def)
        {
            foreach (var item in def.Items)
                _builder.StaticData.RegisterItem(MapItem(item));

            foreach (var faction in def.Factions)
                _builder.StaticData.RegisterFaction(MapFaction(faction));

            foreach (var quest in def.Quests)
                _builder.StaticData.RegisterQuest(MapQuest(quest));
        }

        private static ItemDef MapItem(YamlItemDef d) => new()
        {
            Id          = d.Id,
            Name        = d.Name,
            Kind        = d.Kind,
            MaxStack    = d.MaxStack,
            Weight      = d.Weight,
            BaseValue   = d.BaseValue,
            Description = d.Description
        };

        private static FactionDef MapFaction(YamlFactionDef d) => new()
        {
            Id                = d.Id,
            Name              = d.Name,
            Description       = d.Description,
            FriendlyThreshold = d.FriendlyThreshold,
            HostileThreshold  = d.HostileThreshold
        };

        private static QuestDef MapQuest(YamlQuestDef d) => new()
        {
            Id          = d.Id,
            Name        = d.Name,
            Description = d.Description,
            Objectives  = d.Objectives.Select(MapObjective).ToArray(),
            Rewards     = d.Rewards.Select(MapReward).ToArray()
        };

        private static QuestObjectiveDef MapObjective(YamlQuestObjectiveDef d) => new()
        {
            Id            = d.Id,
            Kind          = d.Kind,
            Description   = d.Description,
            TargetId      = d.TargetId,
            RequiredCount = d.RequiredCount
        };

        private static QuestRewardDef MapReward(YamlQuestRewardDef d) => new()
        {
            ItemId   = d.ItemId,
            Quantity = d.Quantity
        };

        // ── Object construction ──────────────────────────────────────────────────

        private void AddStaticObject(YamlStaticObjDef def)
        {
            _builder.World.AddObject(new WorldObjectStatic
            {
                Kind   = def.Kind,
                Bounds = new Bounds(def.X, def.Y, def.X + def.W, def.Y + def.H)
            });
        }

        private void AddCarryable(YamlCarryableDef def)
        {
            var itemDef = _builder.StaticData.GetItem(def.ItemId)
                ?? throw new InvalidOperationException(
                    $"Unknown item id '{def.ItemId}' in carryable definition. " +
                    $"Ensure it is declared under static_data.items first.");

            _builder.World.AddObject(new WorldObjectCarryable
            {
                Kind     = WorldObjectKind.Item,
                Bounds   = new Bounds(def.X, def.Y, def.X + 1, def.Y + 1),
                ItemDef  = itemDef,
                Quantity = def.Quantity
            });
        }

        // ── NPC dispatch ─────────────────────────────────────────────────────────

        private static void ApplyNpc(Templates t, YamlNpcDef npc)
        {
            var p = npc.Params;
            switch (npc.BehaviorType.ToLowerInvariant())
            {
                case "blacksmith":
                    t.AddBlacksmith(npc.X, npc.Y,
                        shopX: GetInt(p, "shop_x"),
                        shopY: GetInt(p, "shop_y"));
                    break;

                case "farmer":
                    t.AddFarmer(npc.X, npc.Y,
                        wanderRadius: GetInt(p, "wander_radius", 6));
                    break;

                case "guard":
                    t.AddGuard(npc.X, npc.Y,
                        patrolRadius: GetInt(p, "patrol_radius", 4));
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unknown NPC behavior_type: '{npc.BehaviorType}'. " +
                        $"Supported types: blacksmith, farmer, guard.");
            }
        }

        private static int GetInt(
            Dictionary<string, string> p,
            string key,
            int defaultValue = 0)
        {
            if (p.TryGetValue(key, out var val))
                return int.Parse(val);
            return defaultValue;
        }
    }
}

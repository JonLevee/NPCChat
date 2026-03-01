using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using NPCChatLib.Attributes;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace NPChat.CharacterClasses
{
    public enum TraitName
    {
        Warmth,
        Confidence,
        Gossip,
        Magical
    }

    public class Trait
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Dictionary<string, int> Weights { get; set; } = new Dictionary<string, int>();
    }


    public class TraitModifier
    {
        public string TraitName { get; set; } = string.Empty;
        public int Modifier { get; set; } = 0;
    }

    public class CharacterPersonality
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [YamlMember(Alias = "foo")]
        public List<TraitModifier> TraitModifiers { get; set; } = new List<TraitModifier>();
    }

    public class CharacterCoreMetadata
    {
        public List<string> Traits { get; set; } = new List<string>();
        public Dictionary<string, CharacterPersonality> Archetypes { get; set; } = new Dictionary<string, CharacterPersonality>();
    }

    public class Character
    {

    }

    [Transient]
    public class CharacterCreator
    {
        private readonly Character character = new();

        public CharacterCreator FromArchetype(string archetypeName = "Default")
        {
            return this;
        }

        public Character Build()
        {
            return character;
        }
    }

    public enum TraitId : byte
    {
        Friendliness = 0,
        Irritability = 1,
        Trust = 2,
        Confidence = 3,
        Fatigue = 4,
        // ...
        Count
    }


    public struct NpcMood
    {
        public readonly float[] Traits;
        public NpcMood()
        {
            Traits = new float[Enum.GetNames<TraitId>().Length];
        }
    }

    public struct TraitWeight
    {
        public byte TraitIndex;   // maps to TraitId
        public float Weight;      // -1..1 or 0..1
    }

    public struct ResponseDef
    {
        public int TextId;            // points into string table (managed world)
        public ushort WeightStart;    // index into global TraitWeight array
        public byte WeightCount;      // number of weights for this response

        // Optional gating as bitmasks:
        public ushort TopicMask;
        public ushort ShopMask;

        // Optional hard gates:
        public byte MinTrust_0_255;   // trust threshold encoded 0..255
        public byte MaxIrrit_0_255;   // irritability ceiling encoded 0..255

        // Cooldown/freshness bookkeeping keys:
        public ushort CooldownGroupId;
    }

    public struct TraitRange
    {
        public byte TraitIndex;
        public byte Min_0_255;
        public byte Max_0_255;
        public byte Curve; // 0=hard, 1=linear fade, 2=gaussian-ish, etc.
    }

    public unsafe struct NpcDialogueMemory
    {
        public const int RecentCap = 16;
        public byte RecentCount;
        public fixed ushort RecentResponseIds[RecentCap];
        public fixed byte RecentAges[RecentCap]; // cheap decay counter
    }

    public class Globals
    {
        //public TraitWeight[] AllTraitWeights { get; }

        public Globals()
        {
            //AllTraitWeights = new TraitWeight[1000]; // pre-allocated global array of all trait weights for all responses
        }
        public static float ScoreDot(in NpcMood mood, in ResponseDef r, TraitWeight[] weights)
        {
            float s = 0f;
            int start = r.WeightStart;
            int count = r.WeightCount;

            for (int i = 0; i < count; i++)
            {
                var tw = weights[start + i];
                s += mood.Traits[tw.TraitIndex] * tw.Weight;
            }
            return s;
        }

    }
}
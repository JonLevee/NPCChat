using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace NPChat.CharacterClasses
{
    // StockResponses.cs
    // Requires NuGet: YamlDotNet
    // Authoring YAML: mood is Dictionary<string,float> (sparse), requires/forbids are hard gates (0..1 recommended)


#pragma warning disable CS8632
    #region YAML DTOs (authoring-friendly)

    public sealed class YamlRoot
    {
        [YamlMember(Alias = "mood_axes")]
        public List<string> MoodAxes { get; set; } = new();

        [YamlMember(Alias = "stock_responses")]
        public List<YamlStockResponse> StockResponses { get; set; } = new();
    }

    public sealed class YamlStockResponse
    {
        [YamlMember(Alias = "id")]
        public string Id { get; set; } = "";

        [YamlMember(Alias = "text")]
        public string Text { get; set; } = "";

        [YamlMember(Alias = "context")]
        public string Context { get; set; } = "any";

        [YamlMember(Alias = "role")]
        public string Role { get; set; } = "any";

        [YamlMember(Alias = "speaker_target")]
        public string SpeakerTarget { get; set; } = "any"; // pc|npc|any

        [YamlMember(Alias = "intent")]
        public List<string>? Intent { get; set; } // optional list

        [YamlMember(Alias = "mood")]
        public Dictionary<string, float>? Mood { get; set; } // sparse map axis->value

        [YamlMember(Alias = "requires")]
        public Dictionary<string, YamlMinMax>? Requires { get; set; }

        [YamlMember(Alias = "forbids")]
        public Dictionary<string, YamlMinMax>? Forbids { get; set; }

        [YamlMember(Alias = "min_affinity")]
        public float MinAffinity { get; set; } = 0f;

        [YamlMember(Alias = "weight")]
        public float Weight { get; set; } = 1f;

        [YamlMember(Alias = "cooldown")]
        public YamlCooldown? Cooldown { get; set; }
    }

    public sealed class YamlMinMax
    {
        [YamlMember(Alias = "min")]
        public float? Min { get; set; }

        [YamlMember(Alias = "max")]
        public float? Max { get; set; }
    }

    public sealed class YamlCooldown
    {
        [YamlMember(Alias = "self_seconds")]
        public float? SelfSeconds { get; set; }

        [YamlMember(Alias = "group")]
        public string? Group { get; set; }

        [YamlMember(Alias = "group_seconds")]
        public float? GroupSeconds { get; set; }
    }

    #endregion

    #region Runtime Models (fast)

    public readonly struct MinMax
    {
        public readonly float Min;
        public readonly float Max;
        public readonly bool HasMin;
        public readonly bool HasMax;

        public MinMax(float min, bool hasMin, float max, bool hasMax)
        {
            Min = min;
            Max = max;
            HasMin = hasMin;
            HasMax = hasMax;
        }

        public bool Pass(float v)
        {
            if (HasMin && v < Min) return false;
            if (HasMax && v > Max) return false;
            return true;
        }

        public bool MatchesForbidden(float v)
        {
            // same as Pass, but caller interprets it as "if it passes -> forbidden"
            return Pass(v);
        }
    }

    public sealed class CooldownSpec
    {
        public float SelfSeconds;        // 0 => none
        public string? Group;            // null/empty => none
        public float GroupSeconds;       // 0 => none
    }

    public sealed class StockResponse
    {
        public string Id = "";
        public string Text = "";
        public string Context = "any";
        public string Role = "any";
        public string SpeakerTarget = "any";

        public string[] Intent = Array.Empty<string>();

        // Normalized response mood vector (unit length)
        public float[] MoodVecUnit = Array.Empty<float>();

        // Hard gates (stat space: recommend 0..1)
        public Dictionary<string, MinMax> Requires = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, MinMax> Forbids = new(StringComparer.OrdinalIgnoreCase);

        // Dot-product threshold after clamping to [0..1]
        public float MinAffinity = 0f;
        public float Weight = 1f;

        public CooldownSpec Cooldown = new();
    }

    public sealed class StockResponsesDb
    {
        public string[] MoodAxes = Array.Empty<string>();
        public Dictionary<string, int> AxisIndex = new(StringComparer.OrdinalIgnoreCase);
        public List<StockResponse> Responses = new();
    }

    #endregion

    #region Loader

    public static class StockResponsesLoader
    {
        public static StockResponsesDb LoadFromFile(string path)
        {
            var yaml = File.ReadAllText(path);

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            var root = deserializer.Deserialize<YamlRoot>(yaml);
            return BuildRuntimeDb(root);
        }

        public static StockResponsesDb LoadFromText(string yamlText)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            var root = deserializer.Deserialize<YamlRoot>(yamlText);
            return BuildRuntimeDb(root);
        }

        private static StockResponsesDb BuildRuntimeDb(YamlRoot root)
        {
            if (root.MoodAxes == null || root.MoodAxes.Count == 0)
                throw new Exception("mood_axes must be present and non-empty.");

            var db = new StockResponsesDb
            {
                MoodAxes = root.MoodAxes.ToArray(),
                Responses = new List<StockResponse>(root.StockResponses?.Count ?? 0)
            };

            for (int i = 0; i < db.MoodAxes.Length; i++)
            {
                var axis = db.MoodAxes[i]?.Trim();
                if (string.IsNullOrWhiteSpace(axis))
                    throw new Exception($"mood_axes contains an empty entry at index {i}.");
                if (db.AxisIndex.ContainsKey(axis))
                    throw new Exception($"Duplicate mood axis '{axis}'.");
                db.AxisIndex[axis] = i;
            }

            foreach (var y in root.StockResponses ?? new List<YamlStockResponse>())
            {
                if (string.IsNullOrWhiteSpace(y.Id))
                    throw new Exception("A stock_response is missing an id.");

                var r = new StockResponse
                {
                    Id = y.Id.Trim(),
                    Text = y.Text ?? "",
                    Context = (y.Context ?? "any").Trim(),
                    Role = (y.Role ?? "any").Trim(),
                    SpeakerTarget = (y.SpeakerTarget ?? "any").Trim(),
                    MinAffinity = y.MinAffinity,
                    Weight = y.Weight,
                    Intent = (y.Intent ?? new List<string>())
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Select(s => s.Trim())
                        .ToArray(),
                    Cooldown = new CooldownSpec
                    {
                        SelfSeconds = y.Cooldown?.SelfSeconds ?? 0f,
                        Group = string.IsNullOrWhiteSpace(y.Cooldown?.Group) ? null : y.Cooldown!.Group!.Trim(),
                        GroupSeconds = y.Cooldown?.GroupSeconds ?? 0f
                    }
                };

                // Convert sparse mood dict -> dense vector -> normalize -> store
                var dense = new float[db.MoodAxes.Length];
                if (y.Mood != null)
                {
                    foreach (var kvp in y.Mood)
                    {
                        var key = kvp.Key?.Trim();
                        if (string.IsNullOrWhiteSpace(key)) continue;

                        if (!db.AxisIndex.TryGetValue(key, out var idx))
                            throw new Exception($"Response '{r.Id}' uses unknown mood axis '{key}'.");

                        dense[idx] = kvp.Value;
                    }
                }
                NormalizeInPlace(dense);
                r.MoodVecUnit = dense;

                // Convert requires/forbids
                r.Requires = ConvertGates(db, y.Requires, r.Id, "requires");
                r.Forbids = ConvertGates(db, y.Forbids, r.Id, "forbids");

                db.Responses.Add(r);
            }

            return db;
        }

        private static Dictionary<string, MinMax> ConvertGates(
            StockResponsesDb db,
            Dictionary<string, YamlMinMax>? src,
            string responseId,
            string blockName)
        {
            var dst = new Dictionary<string, MinMax>(StringComparer.OrdinalIgnoreCase);
            if (src == null) return dst;

            foreach (var kvp in src)
            {
                var axis = kvp.Key?.Trim();
                if (string.IsNullOrWhiteSpace(axis)) continue;

                // Gate axes do NOT have to be in mood_axes; they can include other stats if you want.
                // If you want to enforce that they must exist in mood_axes, uncomment:
                // if (!db.AxisIndex.ContainsKey(axis)) throw new Exception($"Response '{responseId}' {blockName} uses unknown axis '{axis}'.");

                var mm = kvp.Value ?? new YamlMinMax();
                var hasMin = mm.Min.HasValue;
                var hasMax = mm.Max.HasValue;

                if (!hasMin && !hasMax)
                    throw new Exception($"Response '{responseId}' has '{blockName}.{axis}' with neither min nor max.");

                var min = hasMin ? mm.Min!.Value : 0f;
                var max = hasMax ? mm.Max!.Value : 0f;

                if (hasMin && hasMax && min > max)
                    throw new Exception($"Response '{responseId}' has '{blockName}.{axis}' where min > max.");

                dst[axis] = new MinMax(min, hasMin, max, hasMax);
            }

            return dst;
        }

        private static void NormalizeInPlace(float[] v)
        {
            double sumSq = 0;
            for (int i = 0; i < v.Length; i++) sumSq += v[i] * v[i];
            var mag = Math.Sqrt(sumSq);
            if (mag < 1e-9) return; // leave zero-vector as-is
            var inv = 1.0 / mag;
            for (int i = 0; i < v.Length; i++) v[i] = (float)(v[i] * inv);
        }
    }

    #endregion

    #region Helpers: gates, affinity, cooldown, selection

    public static class DialogueHelpers
    {
        /// <summary>
        /// Evaluate hard gates: requires (all must pass), forbids (none may match).
        /// npcStats are expected normalized 0..1 for these gates (recommended).
        /// Missing stat => fail requires, and does NOT trigger forbids.
        /// </summary>
        public static bool PassHardGates(
            StockResponse r,
            IReadOnlyDictionary<string, float> npcStats01)
        {
            // requires: all must pass
            foreach (var req in r.Requires)
            {
                if (!npcStats01.TryGetValue(req.Key, out var v))
                    return false;
                if (!req.Value.Pass(v))
                    return false;
            }

            // forbids: if any matches -> reject
            foreach (var fb in r.Forbids)
            {
                if (!npcStats01.TryGetValue(fb.Key, out var v))
                    continue; // missing can't forbid
                if (fb.Value.MatchesForbidden(v))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Dot-product affinity, clamped to [0..1] (negative = "actively wrong").
        /// Assumes npcMoodUnit and response.MoodVecUnit are unit vectors.
        /// </summary>
        public static float Affinity01(float[] npcMoodUnit, StockResponse r)
        {
            float dot = 0f;
            var a = npcMoodUnit;
            var b = r.MoodVecUnit;
            int n = Math.Min(a.Length, b.Length);
            for (int i = 0; i < n; i++) dot += a[i] * b[i];
            return dot <= 0f ? 0f : dot;
        }

        /// <summary>
        /// Intent filter: if desiredIntent is null/empty, pass everything.
        /// If the response has no intents, treat it as "generic" and pass.
        /// </summary>
        public static bool PassIntent(StockResponse r, string? desiredIntent)
        {
            if (string.IsNullOrWhiteSpace(desiredIntent)) return true;
            if (r.Intent.Length == 0) return true;
            return r.Intent.Any(i => i.Equals(desiredIntent, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    /// Minimal cooldown tracker. You supply current time (seconds) so this works in Unity or tests.
    /// Keys are per-speaker; group cooldown prevents spamming similar lines.
    /// </summary>
    public sealed class CooldownTracker
    {
        private readonly Dictionary<string, double> _lastBySelfKey = new(StringComparer.Ordinal);
        private readonly Dictionary<string, double> _lastByGroupKey = new(StringComparer.Ordinal);

        /// <summary>
        /// Check whether the response is allowed right now, given cooldown settings.
        /// </summary>
        public bool IsOffCooldown(string speakerId, StockResponse r, double nowSeconds)
        {
            // self cooldown
            if (r.Cooldown.SelfSeconds > 0f)
            {
                var key = MakeSelfKey(speakerId, r.Id);
                if (_lastBySelfKey.TryGetValue(key, out var last))
                {
                    if (nowSeconds - last < r.Cooldown.SelfSeconds)
                        return false;
                }
            }

            // group cooldown
            if (!string.IsNullOrWhiteSpace(r.Cooldown.Group) && r.Cooldown.GroupSeconds > 0f)
            {
                var gkey = MakeGroupKey(speakerId, r.Cooldown.Group!);
                if (_lastByGroupKey.TryGetValue(gkey, out var last))
                {
                    if (nowSeconds - last < r.Cooldown.GroupSeconds)
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Record that we used this response now (so future calls respect cooldowns).
        /// </summary>
        public void MarkUsed(string speakerId, StockResponse r, double nowSeconds)
        {
            if (r.Cooldown.SelfSeconds > 0f)
                _lastBySelfKey[MakeSelfKey(speakerId, r.Id)] = nowSeconds;

            if (!string.IsNullOrWhiteSpace(r.Cooldown.Group) && r.Cooldown.GroupSeconds > 0f)
                _lastByGroupKey[MakeGroupKey(speakerId, r.Cooldown.Group!)] = nowSeconds;
        }

        private static string MakeSelfKey(string speakerId, string responseId) => speakerId + "||" + responseId;
        private static string MakeGroupKey(string speakerId, string group) => speakerId + "||G||" + group;
    }

    public static class DialoguePicker
    {
        /// <summary>
        /// End-to-end pick:
        /// - gate context/role/target (cheap)
        /// - gate intent
        /// - hard gates requires/forbids
        /// - cooldown
        /// - affinity/min_affinity
        /// - weight/freshness
        /// - weighted-random among topN using score^exponent
        /// </summary>
        public static StockResponse? PickOne(
            IEnumerable<StockResponse> responses,
            string context,
            string role,
            string speakerTarget,
            string? desiredIntent,
            string speakerId,
            IReadOnlyDictionary<string, float> npcStats01, // for requires/forbids
            float[] npcMoodUnit,                           // for dot product
            Func<string, float>? freshnessByResponseId,     // e.g., based on recent usage; can be null
            CooldownTracker cooldowns,
            double nowSeconds,
            int topN = 8,
            float exponent = 2.0f,
            Random? rng = null)
        {
            rng ??= new Random();

            bool ContextMatch(StockResponse r) =>
                r.Context.Equals("any", StringComparison.OrdinalIgnoreCase) ||
                r.Context.Equals(context, StringComparison.OrdinalIgnoreCase);

            bool RoleMatch(StockResponse r) =>
                r.Role.Equals("any", StringComparison.OrdinalIgnoreCase) ||
                r.Role.Equals(role, StringComparison.OrdinalIgnoreCase);

            bool TargetMatch(StockResponse r) =>
                r.SpeakerTarget.Equals("any", StringComparison.OrdinalIgnoreCase) ||
                r.SpeakerTarget.Equals(speakerTarget, StringComparison.OrdinalIgnoreCase);

            var scored = new List<(StockResponse r, float score)>();

            foreach (var r in responses)
            {
                if (!ContextMatch(r) || !RoleMatch(r) || !TargetMatch(r)) continue;
                if (!DialogueHelpers.PassIntent(r, desiredIntent)) continue;
                if (!DialogueHelpers.PassHardGates(r, npcStats01)) continue;
                if (!cooldowns.IsOffCooldown(speakerId, r, nowSeconds)) continue;

                var affinity = DialogueHelpers.Affinity01(npcMoodUnit, r);
                if (affinity < r.MinAffinity) continue;

                var freshness = freshnessByResponseId?.Invoke(r.Id) ?? 1.0f;
                var final = affinity * r.Weight * freshness;

                if (final > 0f)
                    scored.Add((r, final));
            }

            if (scored.Count == 0) return null;

            scored.Sort((a, b) => b.score.CompareTo(a.score));
            if (scored.Count > topN) scored.RemoveRange(topN, scored.Count - topN);

            // weighted random: w = score^exponent
            double total = 0;
            var weights = new double[scored.Count];
            for (int i = 0; i < scored.Count; i++)
            {
                var w = Math.Pow(scored[i].score, exponent);
                weights[i] = w;
                total += w;
            }

            double roll = rng.NextDouble() * total;
            for (int i = 0; i < scored.Count; i++)
            {
                roll -= weights[i];
                if (roll <= 0)
                    return scored[i].r;
            }

            return scored[^1].r;
        }
    }

    #endregion

    #region Example: building NPC mood vector from stats

    public static class MoodVectorBuilder
    {
        /// <summary>
        /// Build NPC mood vector in the DB axis order.
        /// statSigned could be already in [-1..+1]. Missing => 0 (neutral).
        /// </summary>
        public static float[] BuildNpcMoodUnit(StockResponsesDb db, IReadOnlyDictionary<string, float> statSignedMinus1ToPlus1)
        {
            var v = new float[db.MoodAxes.Length];
            for (int i = 0; i < db.MoodAxes.Length; i++)
            {
                var axis = db.MoodAxes[i];
                if (statSignedMinus1ToPlus1.TryGetValue(axis, out var val))
                    v[i] = val;
                else
                    v[i] = 0f;
            }
            NormalizeInPlace(v);
            return v;
        }

        private static void NormalizeInPlace(float[] v)
        {
            double sumSq = 0;
            for (int i = 0; i < v.Length; i++) sumSq += v[i] * v[i];
            var mag = Math.Sqrt(sumSq);
            if (mag < 1e-9) return;
            var inv = 1.0 / mag;
            for (int i = 0; i < v.Length; i++) v[i] = (float)(v[i] * inv);
        }
    }

    #endregion}
#pragma warning restore CS8632

}

using Microsoft.Extensions.DependencyInjection;
using NPCChat.Core.Extensions;
using NPCChat.Core.WorldClasses;
using NPCChat.Core.YamlImport;

namespace NPCChat.Tests
{
    [TestClass]
    public sealed class YamlWorldLoaderTests : UnitTestBase
    {
        [TestInitialize]
        public void TestInitialize() => TestInitializeBase();

        // ── Helpers ──────────────────────────────────────────────────────────────

        private const string MinimalYaml = @"
static_data:
  items:
    - id: gold_coin
      name: Gold Coin
      kind: Currency
      max_stack: 9999
      weight: 0.01
      base_value: 1
  factions: []
  quests: []
objects:
  player:
    x: 5
    y: 5
  static: []
  npcs: []
  carryable:
    - item_id: gold_coin
      x: 10
      y: 10
      quantity: 3
";

        private const string NpcYaml = @"
static_data:
  items: []
  factions:
    - id: townsfolk
      name: Townsfolk
      description: Ordinary townsfolk.
      friendly_threshold: 50
      hostile_threshold: -25
  quests: []
objects:
  player:
    x: 16
    y: 14
  static: []
  npcs:
    - behavior_type: guard
      x: 10
      y: 10
      params:
        patrol_radius: ""3""
    - behavior_type: farmer
      x: 24
      y: 14
  carryable: []
";

        // ── Tests ────────────────────────────────────────────────────────────────

        [TestMethod]
        public void LoadFromYaml_RegistersStaticData()
        {
            using var scope = Services.CreateScope();
            var loader = scope.ServiceProvider.Get<YamlWorldLoader>()!;
            var staticData = scope.ServiceProvider.Get<NPCChat.Core.LoadingProviderClasses.StaticData>()!;

            loader.LoadFromYaml(MinimalYaml);

            Assert.IsNotNull(staticData.GetItem("gold_coin"));
            Assert.AreEqual("Gold Coin", staticData.GetItem("gold_coin")!.Name);
        }

        [TestMethod]
        public void LoadFromYaml_CreatesPlayerAndCarryable()
        {
            using var scope = Services.CreateScope();
            var loader = scope.ServiceProvider.Get<YamlWorldLoader>()!;
            var handles = scope.ServiceProvider.Get<ObjectHandleManager>()!;

            loader.LoadFromYaml(MinimalYaml);

            // Player (1 moveable) + carryable item = 2 world objects
            Assert.HasCount(2, handles.ActiveHandles);
        }

        [TestMethod]
        public void LoadFromYaml_CreatesNpcs()
        {
            using var scope = Services.CreateScope();
            var loader = scope.ServiceProvider.Get<YamlWorldLoader>()!;
            var handles = scope.ServiceProvider.Get<ObjectHandleManager>()!;

            loader.LoadFromYaml(NpcYaml);

            // Player + 2 guards/farmers = 3 moveables
            Assert.HasCount(3, handles.ActiveHandles);
        }

        [TestMethod]
        public void LoadFromYaml_StaticObjectAdded()
        {
            const string yaml = @"
static_data:
  items: []
  factions: []
  quests: []
objects:
  static:
    - kind: Building
      x: 10
      y: 2
      w: 5
      h: 3
  npcs: []
  carryable: []
";
            using var scope = Services.CreateScope();
            var loader = scope.ServiceProvider.Get<YamlWorldLoader>()!;
            var handles = scope.ServiceProvider.Get<ObjectHandleManager>()!;

            loader.LoadFromYaml(yaml);

            Assert.HasCount(1, handles.ActiveHandles);
        }

        [TestMethod]
        public void LoadFromYaml_UnknownBehaviorType_Throws()
        {
            const string yaml = @"
static_data:
  items: []
  factions: []
  quests: []
objects:
  npcs:
    - behavior_type: wizard
      x: 5
      y: 5
  static: []
  carryable: []
";
            using var scope = Services.CreateScope();
            var loader = scope.ServiceProvider.Get<YamlWorldLoader>()!;

            bool threw = false;
            try { loader.LoadFromYaml(yaml); }
            catch (InvalidOperationException) { threw = true; }
            Assert.IsTrue(threw, "Expected InvalidOperationException for unknown behavior_type.");
        }

        [TestMethod]
        public void LoadFromYaml_UnknownItemId_Throws()
        {
            const string yaml = @"
static_data:
  items: []
  factions: []
  quests: []
objects:
  static: []
  npcs: []
  carryable:
    - item_id: nonexistent_item
      x: 5
      y: 5
      quantity: 1
";
            using var scope = Services.CreateScope();
            var loader = scope.ServiceProvider.Get<YamlWorldLoader>()!;

            bool threw = false;
            try { loader.LoadFromYaml(yaml); }
            catch (InvalidOperationException) { threw = true; }
            Assert.IsTrue(threw, "Expected InvalidOperationException for unknown item id.");
        }

        [TestMethod]
        public void LoadFromYaml_RegistersFaction()
        {
            using var scope = Services.CreateScope();
            var loader = scope.ServiceProvider.Get<YamlWorldLoader>()!;
            var staticData = scope.ServiceProvider.Get<NPCChat.Core.LoadingProviderClasses.StaticData>()!;

            loader.LoadFromYaml(NpcYaml);

            var faction = staticData.GetFaction("townsfolk");
            Assert.IsNotNull(faction);
            Assert.AreEqual(50, faction!.FriendlyThreshold);
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using NPCChatLib.Builders;
using NPCChatLib.Extensions;
using NPCChatLib.WorldBuilderTemplates;
using NPCChatLib.WorldClasses;

namespace TestProject
{

    [TestClass]
    public class UnitTest1 : UnitTestBase
    {
        [TestInitialize]
        public void TestInitialize()
        {
            TestInitializeBase();
        }

        [TestMethod]
        public void TestMethod1()
        {
            Assert.IsNotNull(Data);
            var expected = new Dictionary<string, byte>();
            var expectedGates = new List<byte>();
            var expectedMoods = new List<byte>();
            List<byte> expectedDisposition = [];
            var dispositionText = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "dispositions.yaml"));
            foreach (var line in dispositionText.Split(Environment.NewLine).Select(t => t.Trim()))
            {
                if (line.Equals("dispositions:")) continue;
                if (line.Equals("gates:"))
                {
                    expectedDisposition = expectedGates;
                    continue;
                }
                if (line.Equals("moods:"))
                {
                    expectedDisposition = expectedMoods;
                    continue;
                }
                if (line.StartsWith("- "))
                {
                    var disposition = line.Substring(2).Trim();
                    var id = (byte)(expected.Count + 1);
                    expected.Add(disposition, id);
                    expectedDisposition.Add(id);
                    continue;
                }

                Assert.IsTrue(string.IsNullOrWhiteSpace(line));
            }
            Assert.HasCount(expected.Count, Data.DispositionIds);
            expected.Keys.IsEquivalentTo(Data.DispositionIds.Keys);
            expected.Values.IsEquivalentTo(Data.DispositionIds.Values);
            expectedGates.IsEquivalentTo(Data.GateIds);
            expectedMoods.IsEquivalentTo(Data.MoodIds);
        }

        [TestMethod]
        public void CreateSmallMap()
        {
            using (IServiceScope scope = Services.CreateScope())
            {
                var options = scope.ServiceProvider.Get<WorldOptions>();
                options.ChunkSize = 16;
                var world = scope.ServiceProvider.Get<WorldData>();
                var builder = scope.ServiceProvider.Get<WorldDataBuilder>();
                var handleManager = scope.ServiceProvider.Get<ObjectHandleManager>();
                Assert.IsNotNull(builder);
                Assert.IsNotNull(builder.World);
                Assert.IsNotNull(builder.Options);
                Assert.IsNotNull(handleManager);

                using (var template = scope.Get<Templates>())
                {
                    template
                        .AddShop(3, 2, BuildingSize.Small)
                        .AddShop(10, 2, BuildingSize.Small);
                }
                Assert.HasCount(2, handleManager.ActiveHandles);
                Assert.HasCount(2, handleManager.Slots);
                Assert.HasCount(0, handleManager.FreeSlotIds);
            }
        }
    }



    //[TestClass]
    //public sealed class Test1
    //{
    //    [AssemblyInitialize]
    //    public static void AssemblyInit(TestContext context)
    //    {
    //        // This method is called once for the test assembly, before any tests are run.
    //    }

    //    [AssemblyCleanup]
    //    public static void AssemblyCleanup()
    //    {
    //        // This method is called once for the test assembly, after all tests are run.
    //    }

    //    [ClassInitialize]
    //    public static void ClassInit(TestContext context)
    //    {
    //        // This method is called once for the test class, before any tests of the class are run.
    //    }

    //    [ClassCleanup]
    //    public static void ClassCleanup()
    //    {
    //        // This method is called once for the test class, after all tests of the class are run.
    //    }

    //    [TestInitialize]
    //    public void TestInit()
    //    {
    //        // This method is called before each test method.
    //    }

    //    [TestCleanup]
    //    public void TestCleanup()
    //    {
    //        // This method is called after each test method.
    //    }

    //    [TestMethod]
    //    public void TestMethod1()
    //    {
    //    }
    //}
}

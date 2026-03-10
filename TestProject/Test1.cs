using System.Drawing;
using Microsoft.Extensions.DependencyInjection;
using NPCChatLib.Builders;
using NPCChatLib.CharacterClasses.Builders;
using NPCChatLib.Extensions;
using NPCChatLib.LoadingProviderClasses;
using NPCChatLib.YamlImport;
using NPChat;
using NPChat.CharacterClasses;

namespace TestProject
{
    public class UnitTestBase
    {
        private IServiceProvider? _serviceProvider;
        protected IServiceProvider Services => _serviceProvider.NonNull();
        protected GlobalDataContainer Data { get; private set; } = null!;
        protected LoadingProviderFactory LoadingFactory { get; private set; } = null!;

        [TestInitialize]
        public void TestInitializeBase()
        {
            IServiceCollection services = new ServiceCollection();
            ConfigureServices.Configure(services);
            _serviceProvider = services.BuildServiceProvider();
            LoadingFactory = Services.Get<LoadingProviderFactory>();
        }
    }

    [TestClass]
    public partial class UnitTest1 : UnitTestBase
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
        public void TestMethod2()
        {
            using (var scope = Services.CreateScope())
            {
                var worldYaml = scope
                    .Get<WorldGenerator>()
                    .SetOptions(s =>
                    {
                        s.LocatorStrategy = NextOpenSpaceLocatorStrategy.Clockwise;
                        s.WorldSize = new Size(20, 20);
                        s.DefaultShopSize = new Size(3, 3);
                        s.DefaultPeoplePerShop = 2;
                    })
                    .GenerateDefault()
                    .Builder
                    .Build();
                Assert.IsNotNull(worldYaml);
                LoadingFactory.Save(worldYaml);
            }
        }

        // Use the UITestMethod attribute for tests that need to run on the UI thread.
        // [UITestMethod]
        public void TestMethod3()
        {
            var metadata = Services!.GetRequiredService<CharacterCoreMetadata>();
            Assert.IsNotNull(metadata);
        }
    }

    public static class UnitTestExtensions
    {
        public static void IsEquivalentTo<T1, T2>(this IEnumerable<T1> left, IEnumerable<T2> right)
        {
            var sortedLeft = left.Order().ToArray();
            var sortedRight = right.Order().ToArray();
            CollectionAssert.AreEquivalent(sortedLeft, sortedRight);
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

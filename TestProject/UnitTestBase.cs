using Microsoft.Extensions.DependencyInjection;
using NPCChatLib.Extensions;
using NPCChatLib.LoadingProviderClasses;
using NPChat;

namespace TestProject
{
    public class UnitTestBase
    {
        private IServiceProvider? _serviceProvider;
        protected IServiceProvider Services { get; private set; } = null!;
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

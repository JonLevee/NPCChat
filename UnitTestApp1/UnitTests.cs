using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;
using NPChat;
using NPChat.CharacterClasses;

namespace UnitTestApp1
{
    [TestClass]
    public class UnitTestBase
    {
        protected IServiceProvider? _services;

        [TestInitialize]
        public void TestInitialize()
        {
            var services = new ServiceCollection();
            // Register types needed by tests. Example: allow resolving UI controls or test services.
            ConfigureServices.Configure(services);
            _services = services.BuildServiceProvider();
        }
    }

    [TestClass]
    public partial class UnitTest1 : UnitTestBase
    {


        [TestMethod]
        public void TestMethod1()
        {
            Assert.IsNotNull(_services);
        }

        // Use the UITestMethod attribute for tests that need to run on the UI thread.
        [UITestMethod]
        public void TestMethod2()
        {
            var metadata = _services!.GetRequiredService<CharacterCoreMetadata>();
            Assert.IsNotNull(metadata);
        }
    }
}

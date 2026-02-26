using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;
using NPChat.CharacterClasses;

namespace UnitTestApp1
{

    [TestClass]
    public partial class UnitTest1 : UnitTestBase
    {


        [TestMethod]
        public void TestMethod1()
        {
            
            var character = Services
                .GetService<CharacterCreator>()?
                .FromArchetype()
                .Build();
            Assert.IsNotNull(character);
        }

        // Use the UITestMethod attribute for tests that need to run on the UI thread.
        // [UITestMethod]
        public void TestMethod2()
        {
            var metadata = Services!.GetRequiredService<CharacterCoreMetadata>();
            Assert.IsNotNull(metadata);
        }
    }
}

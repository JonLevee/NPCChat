using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NPCChatLib.Extensions;
using NPCChatLib.LoadingProviderClasses;
using NPChat;
using NPChat.CharacterClasses;
using System;

namespace UnitTestApp1
{
    public class UnitTestBase
    {
        private IServiceProvider? _serviceProvider;
        protected IServiceProvider Services => _serviceProvider.NonNull();

        [TestInitialize]
        public void TestInitialize()
        {
            var services = new ServiceCollection();
            ConfigureServices.Configure(services);
            _serviceProvider = services.BuildServiceProvider();
            var factory = Services.Get<LoadingProviderFactory>();
            factory.LoadAll();
        }
    }
}

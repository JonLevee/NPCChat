using Microsoft.Extensions.DependencyInjection;
using NPCChatLib.Attributes;
using NPCChatLib.Extensions;
using NPChat.CharacterClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPCChatLib.LoadingProviderClasses
{
    public interface ILoadingProvider
    {
        Type TargetType { get; }
        void Load(IServiceProvider service);
    }
    public abstract class LoadingProviderBase<T> : ILoadingProvider
    {
        public Type TargetType => typeof(T);
        public T Instance { get; set; }
        public abstract void Load(T instance);

        public void Load(IServiceProvider service)
        {
            var instance = service.Get<T>();
            Load(instance);
        }
    }

    [Singleton]
    public class LoadingProviders
    {
        private readonly List<ILoadingProvider> providers;
        private readonly IServiceProvider services;

        public LoadingProviders(IServiceProvider services)
        {
            this.services = services;
            providers = [.. services.GetServices<ILoadingProvider>()];
        }

        public void Load()
        {
            for (int i = 0; i < providers.Count; i++)
            {
                ILoadingProvider provider = providers[i];
                provider.Load(services);
            }

        }
    }

    [Transient]
    public class CharacterCoreMetadataLoadingProvider : LoadingProviderBase<CharacterCoreMetadata>
    {
        public override void Load(CharacterCoreMetadata instance)
        {
            throw new NotImplementedException();
            // Load CharacterCoreMetadata from YAML or other source
        }
    }

    [Transient]
    public class CharacterArchetypesLoadingProvider : LoadingProviderBase<CharacterArchetypes>
    {
        public override void Load(CharacterArchetypes instance)
        {
            throw new NotImplementedException();
        }
    }
}

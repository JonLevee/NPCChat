using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NPCChatLib.Attributes;
using NPCChatLib.Extensions;
using NPChat.CharacterClasses;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

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
    public class LoadingProviderFactory
    {
        private class LoadDataInfo
        {
            public string Name { get; set; } = string.Empty;
            public Action LoadAction { get; set; } = () => { };
        }
        private delegate void LoadData();
        private readonly Lazy<IEnumerable<LoadData>> _loaders;
        private readonly IServiceProvider services;
        private readonly IDeserializer deserializer;
        private GlobalDataContainer globalDataContainer;

        public LoadingProviderFactory(IServiceProvider services)
        {
            _loaders = new Lazy<IEnumerable<LoadData>>(GetLoaders, false);
            this.services = services;
            deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();
        }

        public void LoadAll()
        {
            foreach (var load in _loaders.Value)
            {
                load();
            }
        }
        private IEnumerable<LoadData> GetLoaders()
        {
            yield return LoadMoodData;
        }

        private void LoadMoodData()
        {
            var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "mood_axes.yaml");
            if (!File.Exists(file))
            {
                throw new FileNotFoundException($"Mood axes data file not found: {file}");
            }
            var yaml = File.ReadAllText(file);
            var root = deserializer.Deserialize<MoodAxesYaml>(yaml);

            var loader = services.GetService<LoadingProviderBase<CharacterCoreMetadata>>()!;
            loader.Load(loader.Instance);
        }

        public sealed class MoodAxesYaml
        {
            [YamlMember(Alias = "mood_axes")]
            public List<string> MoodAxes { get; set; } = new();
        }


    }

    [Singleton]
    public class GlobalDataContainer
    {
    }
}

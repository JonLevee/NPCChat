using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NPCChatLib.Attributes;
using NPCChatLib.Extensions;
using NPChat.CharacterClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using static NPCChatLib.LoadingProviderClasses.LoadingProviderFactory;

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
        private delegate void LoadData();
        private readonly LoadDataInfo[] _loadDataInfos;
        private readonly IServiceProvider services;
        private readonly IDeserializer deserializer;
        private GlobalDataContainer globalDataContainer;

        public LoadingProviderFactory(IServiceProvider services)
        {
            _loadDataInfos =
                [
                    new LoadDataInfo("mood_axes.yaml", typeof(MoodAxesYaml), LoadMoodData),
                ];

            this.services = services;
            globalDataContainer = services.Get<GlobalDataContainer>() ?? throw new InvalidOperationException("GlobalDataContainer not registered in service provider");
            deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();
        }

        public void LoadAll()
        {
            foreach (var loadDataInfo in _loadDataInfos)
            {
                var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", loadDataInfo.YamlFileName);
                if (!File.Exists(file))
                {
                    throw new FileNotFoundException($"data file not found: {file}");
                }
                var yaml = File.ReadAllText(file);
                var info = deserializer.Deserialize(yaml, loadDataInfo.TargetType);
                var temp = deserializer.Deserialize(yaml);
                Function<Dictionary<string, byte>> getStringDictionary = () =>
                {
                    var dict = new Dictionary<string, byte>();
                    if (info is MoodAxesYaml moodAxesInfo)
                    {
                        foreach (var axis in moodAxesInfo.MoodAxes)
                        {
                            dict[axis] = 0;
                        }
                    }
                    return dict;
                };
                globalDataContainer.MoodAxes = getStringDictionary(); 

            }
        }

        private IEnumerable<string> GetStrings()
        {

        }

        
        private void LoadMoodData()
        {
        }

        public sealed class MoodAxesYaml
        {
            [YamlMember(Alias = "mood_axes")]
            public List<string> MoodAxes { get; set; } = new();
        }

        private class LoadDataInfo
        {
            public string YamlFileName { get; set; } = string.Empty;
            public Type TargetType { get; set; } = typeof(object);
            public Action LoadAction { get; set; } = () => { };
            public LoadDataInfo(string yamlFileName, Type targetType, Action loadAction)
            {
                YamlFileName = yamlFileName;
                TargetType = targetType;
                LoadAction = loadAction;
            }
        }

    }

    [Singleton]
    public class GlobalDataContainer
    {
        public Dictionary<string,byte> MoodAxes { get; set; } = [];
    }
}

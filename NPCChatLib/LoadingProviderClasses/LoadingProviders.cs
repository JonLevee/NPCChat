using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NPCChatLib.Attributes;
using NPCChatLib.Extensions;
using NPCChatLib.YamlImport;
using NPChat.CharacterClasses;
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
        private readonly IServiceProvider services;
        private readonly IDeserializer deserializer;
        private GlobalDataContainer globalDataContainer;

        public LoadingProviderFactory(IServiceProvider services)
        {
            this.services = services;
            globalDataContainer = services.Get<GlobalDataContainer>() ?? throw new InvalidOperationException("GlobalDataContainer not registered in service provider");
            deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();
        }

        public void LoadAll()
        {
            // Load("mood_axes.yaml", d => SetDictionary(globalDataContainer.MoodAxes, d["mood_axes"] as List<object>));
            //Load("mood_axes.yaml", s => s as List<object>, g => g.MoodAxes, SetDictionary);
            using (var moodAxesImport = GetImportedYaml<YamlImportMoodAxes>("dispositions.yaml"))
            {
                var dispositionIds = new Dictionary<string, byte>();
                var moodIds = new List<byte>();
                var gateIds = new List<byte>();
                foreach (var gate in moodAxesImport.Dispositions["gates"])
                {
                    var id = (byte)(dispositionIds.Count + 1);
                    gateIds.Add(id);
                    dispositionIds.Add(gate, id);
                }
                foreach (var mood in moodAxesImport.Dispositions["moods"])
                {
                    var id = (byte)dispositionIds.Count;
                    moodIds.Add(id);
                    dispositionIds.Add(mood, id);
                }
                globalDataContainer.DispositionIds = dispositionIds;
                globalDataContainer.MoodIds = moodIds.ToArray();
                globalDataContainer.GateIds = gateIds.ToArray();
            }
        }

        private T GetImportedYaml<T>(string yamlFile) where T
        {
            var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", yamlFile);
            if (!File.Exists(file))
            {
                throw new FileNotFoundException($"data file not found: {file}");
            }
            var yaml = File.ReadAllText(file);
            var yamlObject = deserializer.Deserialize<T>(yaml);
            return yamlObject;
        }

        private void Load<SType, TType>(
            string yamlFile,
            Func<object, SType> getSource,
            Func<GlobalDataContainer, TType> getTarget,
            Action<SType, TType> loader,
            string yamlStartTag = null)
        {
            yamlStartTag = yamlStartTag ?? Path.GetFileNameWithoutExtension(yamlFile);
            var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", yamlFile);
            if (!File.Exists(file))
            {
                throw new FileNotFoundException($"data file not found: {file}");
            }
            var yaml = File.ReadAllText(file);
            var yamlObject = deserializer.Deserialize(yaml) as Dictionary<object, object>;
            var startingObject = yamlObject[yamlStartTag];
            var source = getSource(startingObject);
            var target = getTarget(globalDataContainer);
            loader(source, target);
        }

        //private void Load(string yamlFile, Action<Dictionary<object, object>> loader)
        //{
        //    var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", yamlFile);
        //    if (!File.Exists(file))
        //    {
        //        throw new FileNotFoundException($"data file not found: {file}");
        //    }
        //    var yaml = File.ReadAllText(file);
        //    var rawYamlObject = deserializer.Deserialize(yaml);
        //    var yamlObject = rawYamlObject as Dictionary<object, object>;
        //    loader(yamlObject);
        //}

        private void SetDictionary<T>(List<object> source, Dictionary<string, T> target)
        {
            for (int i = 0; i < source.Count; i++)
            {
                var axis = source[i] as string;
                target[axis] = (T)Convert.ChangeType(i, typeof(T));
            }
        }
    }
}

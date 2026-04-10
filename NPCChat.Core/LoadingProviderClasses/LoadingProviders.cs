using System.Runtime.CompilerServices;
using NPCChat.Core.Attributes;
using NPCChat.Core.Extensions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace NPCChat.Core.LoadingProviderClasses
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
        private readonly ISerializer serializer;
        private StaticData staticData;
        private string[] persistanceLocations = [
            @"C:\Users\jonle\My Drive\Games\YamlData"
            ];

        public LoadingProviderFactory(IServiceProvider services)
        {
            this.services = services;
            staticData = services.Get<StaticData>() ?? throw new InvalidOperationException("StaticData not registered in service provider");
            deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();
            serializer = new SerializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();
        }

        public void LoadAll()
        {
            // Load("mood_axes.yaml", d => SetDictionary(globalDataContainer.MoodAxes, d["mood_axes"] as List<object>));
            //Load("mood_axes.yaml", s => s as List<object>, g => g.MoodAxes, SetDictionary);
            throw new NotImplementedException();
            //using (var moodAxesImport = GetImportedYaml<YamlImportMoodAxes>("dispositions.yaml"))
            //{
            //    var dispositionIds = new Dictionary<string, byte>();
            //    var moodIds = new List<byte>();
            //    var gateIds = new List<byte>();
            //    foreach (var gate in moodAxesImport.Dispositions["gates"])
            //    {
            //        var id = (byte)(dispositionIds.Count + 1);
            //        gateIds.Add(id);
            //        dispositionIds.Add(gate, id);
            //    }
            //    foreach (var mood in moodAxesImport.Dispositions["moods"])
            //    {
            //        var id = (byte)dispositionIds.Count;
            //        moodIds.Add(id);
            //        dispositionIds.Add(mood, id);
            //    }
            //    globalDataContainer.DispositionIds = dispositionIds;
            //    globalDataContainer.MoodIds = moodIds.ToArray();
            //    globalDataContainer.GateIds = gateIds.ToArray();
            //}
        }

        public void Save<T>(
            T item,
            [CallerMemberName]
            string callerName = null
            )
        {
            var name = typeof(T).Name;
            var outputFile = Path.Combine(persistanceLocations.First(Directory.Exists), name + ".yaml");
            var text = serializer.Serialize(item);
            File.WriteAllText(outputFile, text);
        }

        private T GetImportedYaml<T>(string yamlFile) where T : class
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
            Func<StaticData, TType> getTarget,
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
            var target = getTarget(staticData);
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

using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using NPCChat.Core;
using NPCChat.Core.WorldClasses;
using NPCChat.Core.YamlImport;
using UnityEngine;

/// <summary>
/// Owns the DI container, loads YAML, and starts/stops the simulation.
/// Place on a single "Bootstrap" GameObject in the scene.
/// All other scripts access the simulation through Instance.World.
/// </summary>
public class SimulationBootstrap : MonoBehaviour
{
    public static SimulationBootstrap Instance { get; private set; }

    public WorldData World { get; private set; }
    public bool IsReady { get; private set; }

    private IServiceProvider _rootProvider;
    private IServiceScope _scope;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        var services = new ServiceCollection();
        ConfigureServices.Configure(services);
        _rootProvider = services.BuildServiceProvider();
        _scope = _rootProvider.CreateScope();
        World = _scope.ServiceProvider.GetRequiredService<WorldData>();
    }

    private void Start()
    {
        var loader = _scope.ServiceProvider.GetRequiredService<YamlWorldLoader>();
        var path = Path.Combine(Application.streamingAssetsPath, "Data", "world.yaml");
        loader.LoadFromFile(path);
        World.StartSimulationProcessing();
        IsReady = true;
    }

    private void OnDestroy()
    {
        if (Instance != this) return;
        World?.StopSimulationProcessing();
        World?.Dispose();
        _scope?.Dispose();
        if (_rootProvider is IDisposable d) d.Dispose();
        Instance = null;
    }
}

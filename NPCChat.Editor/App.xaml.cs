using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using NPCChat.Core;
using Application = System.Windows.Application;

namespace NPCChat.Editor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; } = null!;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Create a ServiceCollection
            IServiceCollection services = new ServiceCollection();

            // 2. Register your services, view models, and windows
            ConfigureServices.Configure(services);
            services.AddSingleton<MainWindow>();

            // 3. Build the IServiceProvider
            Services = services.BuildServiceProvider();

            // 4. Resolve and show the main window
            var mainWindow = Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }
}


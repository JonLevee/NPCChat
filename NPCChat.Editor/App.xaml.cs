using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using NPCChat.Core;

namespace NPCChat.Editor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IServiceProvider _serviceProvider = null!;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Create a ServiceCollection
            IServiceCollection services = new ServiceCollection();

            // 2. Register your services, view models, and windows
            ConfigureServices.Configure(services);
            services.AddSingleton<MainWindow>();

            // 3. Build the IServiceProvider
            _serviceProvider = services.BuildServiceProvider();

            // 4. Resolve and show the main window
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }


        // Optional: Provide a way to access the ServiceProvider from other parts of the application if necessary
        public static IServiceProvider ServiceProvider => (Current as App)?._serviceProvider!;
    }
}


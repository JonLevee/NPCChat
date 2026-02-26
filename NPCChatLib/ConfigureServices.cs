using Microsoft.Extensions.DependencyInjection;
using NPCChatLib.Attributes;
using NPCChatLib.Extensions;
using NPChat.CharacterClasses;
using System.Linq;
using System.Reflection;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace NPChat
{
    public static class ConfigureServices
    {
        public static void Configure(IServiceCollection services)
        {
            Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.GetCustomAttribute<InjectionAttribute>() != null)
                .ForEach(t =>
                {
                    var attr = t.GetCustomAttribute<InjectionAttribute>();
                    if (attr != null)
                    {
                        switch (attr.Lifetime)
                        {
                            case ServiceLifetime.Singleton:
                                services.AddSingleton(t);
                                break;
                            case ServiceLifetime.Scoped:
                                services.AddScoped(t);
                                break;
                            case ServiceLifetime.Transient:
                                services.AddTransient(t);
                                break;
                        }
                    }
                });
        }
    }
}


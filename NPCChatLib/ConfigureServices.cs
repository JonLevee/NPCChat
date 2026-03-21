using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NPCChatLib.Attributes;
using NPCChatLib.Extensions;
using NPChat.CharacterClasses;

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
                    attr?.GetServiceDescriptors(t).ForEach(services.Add);
                });
        }
    }
}


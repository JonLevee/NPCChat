using Microsoft.Extensions.DependencyInjection;
using NPChat.CharacterClasses;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace NPChat
{
    public static class ConfigureServices
    {
        public static void Configure(IServiceCollection services)
        {
            services.AddSingleton<CharacterCoreMetadataProvider>();
            services.AddSingleton(sp => sp.GetRequiredService<CharacterCoreMetadataProvider>().GetCharacterCoreMetadata());
        }
    }
}

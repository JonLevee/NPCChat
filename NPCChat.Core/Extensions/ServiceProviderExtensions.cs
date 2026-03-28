using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace NPCChatLib.Extensions
{
    public static class ServiceProviderExtensions
    {
        public static T NonNull<T>(
#pragma warning disable CS8632 // null parameter reference.
            this T? instance,
#pragma warning restore CS8632 
            [CallerMemberName]
            string name = null) where T : class
        {
            if (instance is null)
            {
                throw new InvalidOperationException($"Variable {name ?? "(null)"} of type {typeof(T).Name} is null");
            }
            return instance;
        }
#pragma warning disable CS8632 // null parameter reference.
        public static T Get<T>(this IServiceProvider? serviceProvider) => (T)(serviceProvider?.GetRequiredService(typeof(T)));
        public static T Get<T>(this IServiceScope? scope) => (T)scope?.ServiceProvider?.GetRequiredService(typeof(T));
    }
#pragma warning restore CS8632 
}

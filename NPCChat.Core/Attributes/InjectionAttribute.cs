using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPCChat.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true, AllowMultiple = false)]
    public abstract class InjectionAttribute(ServiceLifetime lifetime, Type serviceType) : Attribute
    {
        public ServiceLifetime Lifetime { get; } = lifetime;
        public Type ServiceType { get; } = serviceType;
        public IEnumerable<ServiceDescriptor> GetServiceDescriptors(Type implementationType)
        {
            yield return new(implementationType, implementationType, Lifetime);
            if (ServiceType != null)
                yield return new(ServiceType, implementationType, Lifetime);
        }
    }

    public class SingletonAttribute(Type serviceType = null) : InjectionAttribute(ServiceLifetime.Singleton, serviceType)
    {
    }

    public class TransientAttribute(Type serviceType = null) : InjectionAttribute(ServiceLifetime.Transient, serviceType)
    {
    }

    public class ScopedAttribute(Type serviceType = null) : InjectionAttribute(ServiceLifetime.Scoped, serviceType)
    {
    }
}

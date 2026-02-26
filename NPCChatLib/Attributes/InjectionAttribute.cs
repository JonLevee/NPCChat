using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPCChatLib.Attributes
{
    public abstract class InjectionAttribute : Attribute
    {
        public InjectionAttribute(ServiceLifetime lifetime) 
        {
            Lifetime = lifetime;
        }

        public ServiceLifetime Lifetime { get; }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class SingletonAttribute : InjectionAttribute
    {
        public SingletonAttribute() : base(ServiceLifetime.Singleton) { }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class TransientAttribute : InjectionAttribute
    {
        public TransientAttribute() : base(ServiceLifetime.Transient) { }
    }
}

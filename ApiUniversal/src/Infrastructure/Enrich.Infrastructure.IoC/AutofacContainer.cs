using Autofac;
using Enrich.Core.Container;
using System;

namespace Enrich.Infrastructure.IoC
{
    public class AutofacContainer : IEnrichContainer
    {
        private readonly ILifetimeScope _container;

        public AutofacContainer(ILifetimeScope container)
        {
            _container = container;
        }
        public T Resolve<T>(bool throwIfNotFound = true)
        {

            if (_container.TryResolve<Autofac.ILifetimeScope>(out var scope) &&
             scope.TryResolve(typeof(T), out var scopeComponent))
                return (T)scopeComponent;

            if (_container.TryResolve(typeof(T), out var component))
                return (T)component;

            return default;

            if (throwIfNotFound)
            {
                throw new Exception($"{typeof(T)} is not register.");
            }

            return default(T);
        }

        public object Resolve(Type type, bool throwIfNotFound = true)
        {
            if (_container.TryResolve(type, out var service))
            {
                return service;
            }

            if (throwIfNotFound)
            {
                throw new Exception($"{type} is not register.");
            }

            return null;
        }

        public T ResolveByKeyed<T>(object keyed, bool defaultIfNotFound = true)
        {
            if (_container.TryResolveKeyed(keyed, typeof(T), out var service))
            {
                return (T)service;
            }

            if (defaultIfNotFound)
            {
                return Resolve<T>();
            }

            return default(T);
        }
    }
}

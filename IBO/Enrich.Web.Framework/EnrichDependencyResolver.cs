using Enrich.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Web.Framework
{
    public class EnrichDependencyResolver : System.Web.Mvc.IDependencyResolver
    {
        public object GetService(Type serviceType)
        {
            return EngineContext.Current.ContainerManager?.ResolveOptional(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            var type = typeof(IEnumerable<>).MakeGenericType(serviceType);
            return (IEnumerable<object>)EngineContext.Current.Resolve(type);
        }

    }
}

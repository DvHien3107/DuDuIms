using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Core.Container
{
    public interface IEnrichContainer
    {
        T Resolve<T>(bool throwIfNotFound = true);

        object Resolve(Type type, bool throwIfNotFound = true);

        T ResolveByKeyed<T>(object keyed, bool defaultIfNotFound = true);
    }
}

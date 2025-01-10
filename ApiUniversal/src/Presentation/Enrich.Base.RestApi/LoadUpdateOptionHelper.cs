using Enrich.Common.Helpers;
using Enrich.Dto.Base.Attributes;
using Microsoft.AspNetCore.JsonPatch.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Base.RestApi
{
    public static class LoadUpdateOptionHelper
    {
        /// <summary>
        /// Get load option by using LoadOrUpdateOptionAttribute
        /// </summary>
        public static TLoad GetLoadOption<T, TLoad>(List<Operation<T>> operations)
            where T : class
            where TLoad : class, new()
        {
            if (operations.Count == 0)
            {
                return new TLoad();
            }

            return ProcessGetLoadOption<TLoad>(propName => operations.Any(a => $"{a.path}/".StartsWithEx($"/{propName}/")));
        }

        /// <summary>
        /// Get load option by using LoadOrUpdateOptionAttribute
        /// </summary>
        public static TLoad GetLoadOption<TLoad>(string propNames, TLoad defValue = default(TLoad))
            where TLoad : class, new()
        {
            if (string.IsNullOrWhiteSpace(propNames))
                return defValue;

            if (propNames.EqualsEx("*"))
                return null; //load all

            var lstPropNames = propNames.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);

            return ProcessGetLoadOption<TLoad>(propName => lstPropNames.Any(a => a.EqualsEx(propName)));
        }

        private static TLoad ProcessGetLoadOption<TLoad>(Func<string, bool> checkHasLoad)
            where TLoad : class, new()
        {
            var load = new TLoad();
            var typeBool = false.GetType();

            foreach (var prop in typeof(TLoad).GetProperties().Where(x => x.PropertyType == typeBool)) //only load boolean fields
            {
                if (prop.TryGetAttribute<LoadOrUpdateOptionAttribute>(out var attr) && !string.IsNullOrWhiteSpace(attr.PropName))
                {
                    var hasLoad = checkHasLoad(attr.PropName);
                    prop.SetValue(load, hasLoad);
                }
            }

            return load;
        }


    }
}

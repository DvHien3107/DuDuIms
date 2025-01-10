using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Services.Implement.Library
{
    public abstract class ImpBaseConfig
    {
        protected async Task<Dictionary<string, object>> ConfigValuesAsync(Dictionary<string, Task> configs, Func<Dictionary<string, Task>> extend = null)
        {
            if (extend != null)
            {
                var extendConfigs = extend();
                foreach (var item in extendConfigs)
                {
                    if (!configs.ContainsKey(item.Key))
                    {
                        configs.Add(item.Key, item.Value);
                    }
                }
            }

            if (configs.Count == 0)
            {
                return new Dictionary<string, object>();
            }

            await Task.WhenAll(configs.Values);

            return configs.ToDictionary(k => k.Key, v => v.Value.GetType().GetProperty("Result")?.GetValue(v.Value));
        }
    }
}

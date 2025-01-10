using Autofac;
using Autofac.Core;
using Enrich.Common;
using Enrich.Core.Infrastructure;
using Enrich.Core.Utils;
using Enrich.Infrastructure.Cache;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.RestApi.NetCorePlatform.Infrastructure
{
    public class EnrichCacheStartup : IEnrichStartup
    {
        private static readonly IMemoryCache _memoryCacheInstance = new Microsoft.Extensions.Caching.Memory.MemoryCache(new MemoryCacheOptions());

        public int Order => 1000;


        public void ConfigureServices(ContainerBuilder builder, IConfiguration config)
        {
            builder.RegisterInstance(_memoryCacheInstance).As<IMemoryCache>().SingleInstance();

            builder.RegisterType<NoCache>().Keyed<IEnrichCache>(Constants.CacheName.NoCache).PreserveExistingDefaults().SingleInstance();
            builder.RegisterType<Enrich.Infrastructure.Cache.MemoryCache>().Keyed<IEnrichCache>(Constants.CacheName.MemoryCache).SingleInstance();
            builder.RegisterType<RedisCache>().Keyed<IEnrichCache>(Constants.CacheName.RedisCache)
                    .WithParameter("configuration", config["Caches:Settings:Redis:ConnectionString"])
                    .WithParameter("database", config["Caches:Settings:Redis:Database"])
                    .WithParameter("cacheNamespace", config["Caches:Settings:Redis:Namespace"])
                    .WithParameter(new ResolvedParameter((pi, ctx) => pi.ParameterType == typeof(IEnrichCache) && pi.Name == "backupCache", (pi, ctx) =>
                    {
                        var backupName = config["Caches:Settings:Redis:BackupCacheName"];
                        return !string.IsNullOrWhiteSpace(backupName) && ctx.TryResolveKeyed(backupName, typeof(IEnrichCache), out var service) ? (IEnrichCache)service : null;
                    })
                    )
                    .SingleInstance();
        }

        public void Configure(IApplicationBuilder application)
        {

        }
       
    }
}

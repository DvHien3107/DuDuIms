using Autofac;
using Autofac.Core;
using Enrich.Common;
using Enrich.Core;
using Enrich.Core.Configs;
using Enrich.Core.Infrastructure;
using Enrich.Core.Utils;
using Enrich.Dto;
using Enrich.IMS.Services.Implement.Library;
using Enrich.IMS.Services.Implement.Services.NextGen.IBO;
using Enrich.Infrastructure.Log;
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
    public class EnrichContextStartup : IEnrichStartup
    {
      
        public int Order => 1000;


        public void ConfigureServices(ContainerBuilder builder, IConfiguration configuration)
        {
            builder.RegisterType<DefaultEnrichContextFactory>().As<IEnrichContextFactory>().SingleInstance();
            builder.RegisterType<EnrichContext>().OnRelease(instance => instance.Release()).InstancePerLifetimeScope();
            builder.RegisterType<IboSyncService>().As<IIboSyncService>().InstancePerLifetimeScope();
        }

        public void Configure(IApplicationBuilder application)
        {

        }
    }
}

using Autofac;
using Autofac.Core;
using Enrich.Common;
using Enrich.Core;
using Enrich.Core.Configs;
using Enrich.Core.Infrastructure;
using Enrich.Core.Utils;
using Enrich.IMS.Services.Implement.Library;
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
    public class EnrichAppConfigStartup : IEnrichStartup
    {
      
        public int Order => 1000;


        public void ConfigureServices(ContainerBuilder builder, IConfiguration configuration)
        {
            //app-settings
            builder.RegisterInstance(configuration).SingleInstance();
            builder.RegisterType<EnrichAppConfig>().As<IAppConfig>();
        }

        public void Configure(IApplicationBuilder application)
        {

        }
      
    }
}

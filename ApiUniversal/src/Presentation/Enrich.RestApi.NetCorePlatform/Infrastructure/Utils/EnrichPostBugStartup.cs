using Autofac;
using Autofac.Core;
using Enrich.Common;
using Enrich.Core;
using Enrich.Core.Infrastructure;
using Enrich.Core.Utils;
using Enrich.Infrastructure.Slack;
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
    public class EnrichPostBugStartup : IEnrichStartup
    {     
        public int Order => 1000;

        public void ConfigureServices(ContainerBuilder builder, IConfiguration configuration)
        {
            builder.RegisterType<EnrichSlack>().As<IEnrichSlack>().WithParameter("token", "xoxb-3408625377281-3638852660820-JO64QRhzrsc8EEf243f3UJve").InstancePerLifetimeScope();
        }

        public void Configure(IApplicationBuilder application)
        {

        }
      
    }
}

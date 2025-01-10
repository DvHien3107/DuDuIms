using Autofac;
using Autofac.Core;
using Enrich.Common;
using Enrich.Core.Container;
using Enrich.Core.Infrastructure;
using Enrich.Core.Utils;
using Enrich.Infrastructure.IoC;
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
    public class EnrichIoCStartup : IEnrichStartup
    {
       

        public int Order => 1000;


        public void ConfigureServices(ContainerBuilder builder, IConfiguration configuration)
        {
            builder.RegisterType<AutofacContainer>().As<IEnrichContainer>().InstancePerLifetimeScope();
        }

        public void Configure(IApplicationBuilder application)
        {

        }
      
    }
}

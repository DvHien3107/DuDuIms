using Autofac;
using Enrich.Core.Infrastructure;
using Enrich.Core.Utils;
using Enrich.Infrastructure.JiraConnector;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.RestApi.NetCorePlatform.Infrastructure.Utils
{
    public class JiraConnectorStartup : IEnrichStartup
    {

        public int Order => 1000;

        public void ConfigureServices(ContainerBuilder builder, IConfiguration configuration)
        {
            builder.RegisterType<JiraConnectorService>().As<IJiraConnectorService>().InstancePerLifetimeScope();
        }

        public void Configure(IApplicationBuilder application)
        {

        }

    }
}

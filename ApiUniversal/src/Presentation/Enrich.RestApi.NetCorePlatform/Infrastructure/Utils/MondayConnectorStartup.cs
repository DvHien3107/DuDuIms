using Autofac;
using Enrich.Core.Infrastructure;
using Enrich.Core.Utils;
using Enrich.Infrastructure.Email;
using Enrich.Infrastructure.MondayConnector;
using Enrich.Infrastructure.SMS;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Enrich.RestApi.NetCorePlatform.Infrastructure
{
    public class MondayConnectorStartup : IEnrichStartup
    {     

        public int Order => 1000;


        public void ConfigureServices(ContainerBuilder builder, IConfiguration configuration)
        {
            builder.RegisterType<MondayConnector>().As<IMondayConnector>().InstancePerLifetimeScope();
        }

        public void Configure(IApplicationBuilder application)
        {

        }

    }
}

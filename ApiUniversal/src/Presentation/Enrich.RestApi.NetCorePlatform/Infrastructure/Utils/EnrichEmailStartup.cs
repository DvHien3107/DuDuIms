using Autofac;
using Enrich.Core.Infrastructure;
using Enrich.Core.Utils;
using Enrich.Infrastructure.Email;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Enrich.RestApi.NetCorePlatform.Infrastructure
{
    public class EnrichEmailStartup : IEnrichStartup
    {     

        public int Order => 1000;


        public void ConfigureServices(ContainerBuilder builder, IConfiguration configuration)
        {
            builder.RegisterType<EnrichSendEmail>().As<IEnrichSendEmail>().InstancePerLifetimeScope();
        }

        public void Configure(IApplicationBuilder application)
        {

        }

    }
}

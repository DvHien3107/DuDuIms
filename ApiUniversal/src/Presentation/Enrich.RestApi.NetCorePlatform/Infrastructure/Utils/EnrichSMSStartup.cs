using Autofac;
using Enrich.Core.Infrastructure;
using Enrich.Core.Utils;
using Enrich.Infrastructure.Email;
using Enrich.Infrastructure.SMS;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Enrich.RestApi.NetCorePlatform.Infrastructure
{
    public class EnrichSMSStartup : IEnrichStartup
    {     

        public int Order => 1000;


        public void ConfigureServices(ContainerBuilder builder, IConfiguration configuration)
        {
            builder.RegisterType<EnrichSms>().As<IEnrichSms>().InstancePerLifetimeScope();
            builder.RegisterType<EnrichSMSService>().As<IEnrichSMSService>().InstancePerLifetimeScope();
        }

        public void Configure(IApplicationBuilder application)
        {

        }

    }
}

using Autofac;
using Autofac.Core;
using Enrich.Common;
using Enrich.Core;
using Enrich.Core.Infrastructure;
using Enrich.Core.Utils;
using Enrich.Payment.MxMerchant;
using Enrich.Payment.MxMerchant.Api;
using Enrich.Payment.MxMerchant.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Enrich.RestApi.NetCorePlatform.Infrastructure
{
    public class EnrichMxMerchantStartup : IEnrichStartup
    {     

        public int Order => 1000;


        public void ConfigureServices(ContainerBuilder builder, IConfiguration configuration)
        {
            builder.RegisterType<PaymentFunction>().As<IPaymentFunction>().InstancePerLifetimeScope();
            builder.RegisterType<MxMerchantFunction>().As<IMxMerchantFunction>().InstancePerLifetimeScope();
            builder.RegisterType<OAuthNew>().As<IOAuthNew>().InstancePerLifetimeScope();
            builder.RegisterType<OAuthUtilities>().As<IOAuthUtilities>().InstancePerLifetimeScope();
            builder.RegisterInstance<ConfigFactory>(configuration.GetSection("Services:MxMerchant").Get<ConfigFactory>() ?? new ConfigFactory()).SingleInstance();
        }

        public void Configure(IApplicationBuilder application)
        {

        }

    }
}

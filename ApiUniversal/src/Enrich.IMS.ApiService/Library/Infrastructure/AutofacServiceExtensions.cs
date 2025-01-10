using Autofac;
using Enrich.Core.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace Enrich.IMS.RestApi.Library.Infrastructure
{
    /// <summary>
    /// Represents extensions of ContainerBuilder (autofac)
    /// </summary>
    public static class AutofacServiceExtensions
    {
        /// <summary>
        /// Add services to the autofac and configure service provider
        /// </summary>

        /// <param name="builder">A builder (container builder)</param>
        /// <param name="configuration"></param>
        public static void ConfigureAutofacServices(this ContainerBuilder builder, IConfiguration configuration)
        {
            //add accessor to HttpContext
            builder.AddHttpContextAccessor();

            //find startup configurations provided by other assemblies
            var typeFinder = new WebAppTypeFinder();
            Singleton<ITypeFinder>.Instance = typeFinder;
            var startupConfigurations = typeFinder.FindClassesOfType<IEnrichStartup>();

            //create and sort instances of startup configurations
            var instances = startupConfigurations
                .Select(startup => (IEnrichStartup)Activator.CreateInstance(startup))
                .OrderBy(startup => startup.Order);

            //configure services
            foreach (var instance in instances)
            {
                instance.ConfigureServices(builder, configuration);
            }
        }

        /// <summary>
        /// Register HttpContextAccessor
        /// </summary>
        /// <param name="builder">ContainerBuilder</param>
        public static void AddHttpContextAccessor(this ContainerBuilder builder)
        {
            builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>().SingleInstance();
        }

    }
}

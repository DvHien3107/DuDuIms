using Autofac;
using Enrich.Core.Infrastructure;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.RestApi.NetCorePlatform.Infrastructure
{

    /// <summary>
    /// Represents extensions of ContainerBuilder (autofac). Dont use this function for web app
    /// It is only use to registy all base service, which come from library
    /// </summary>
    public static class AutofacBaseServiceExtensions
    {
        /// <summary>
        /// Add services to the autofac and configure service provider
        /// </summary>

        /// <param name="builder">A builder (container builder)</param>
        /// <param name="configuration"></param>
        public static void ConfigureAutofacBaseServices(this ContainerBuilder builder, IConfiguration configuration)
        {

            //find startup configurations provided by other assemblies
            var typeFinder = new AppDomainTypeFinder();
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
    }
}

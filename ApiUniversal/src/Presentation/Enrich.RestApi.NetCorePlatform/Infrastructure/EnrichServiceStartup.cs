using Autofac;
using Autofac.Core;
using Enrich.Common;
using Enrich.Core;
using Enrich.Core.Configs;
using Enrich.Core.Infrastructure;
using Enrich.Core.Utils;
using Enrich.Dto;
using Enrich.IMS.Infrastructure.Data.Implement.Repositories;
using Enrich.IMS.Services.Implement.Library;
using Enrich.IMS.Services.Implement.Services;
using Enrich.Infrastructure.Data;
using Enrich.Infrastructure.Data.Dapper;
using Enrich.Infrastructure.Log;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.RestApi.NetCorePlatform.Infrastructure
{
    public class EnrichServiceStartup : IEnrichStartup
    {
      
        public int Order => 1000;


        public void ConfigureServices(ContainerBuilder builder, IConfiguration config)
        {
            RegisterRepositories(builder, config);
            RegisterServices(builder);
        }

        public void Configure(IApplicationBuilder application)
        {

        }

        #region method
      
        private static void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(Assembly<TicketService>()).As(t => GetInterfaceType(t, "IGenericService", "IService"));

        }

        private static void RegisterRepositories(ContainerBuilder builder, Microsoft.Extensions.Configuration.IConfiguration config)
        {
            builder.RegisterInstance(new EnrichIMSDbContext()).SingleInstance();

            var enrichIMSConnectionFactory = new EnrichIMSConnectionFactory(config.GetConnectionString("EnrichcousBOConnectionString"));
            builder.RegisterInstance(enrichIMSConnectionFactory).SingleInstance();

            //Repositories for Management 
            var managementRepositoryBuilderRegistry = builder.RegisterAssemblyTypes(Assembly<EmailTemplateRepository>()).As(t => GetInterfaceType(t, "IGenericRepository", "IRepository"))
                .WithParameter("connectionFactory", enrichIMSConnectionFactory);

        }

        private static Assembly Assembly<T>()
        {
            var result = typeof(T).GetTypeInfo().Assembly;
            return result;
        }

        private static Type GetInterfaceType(Type t, params string[] baseInterfaceNames)
        {
            var result = t.GetInterfaces().Where(i => baseInterfaceNames.All(name => !i.Name.Contains(name))).FirstOrDefault() ?? t;
            return result;
        }

        #endregion
    }
}

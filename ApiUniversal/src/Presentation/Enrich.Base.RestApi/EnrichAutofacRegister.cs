using Autofac;
using Autofac.Core;
using Enrich.Common;
using Enrich.Common.Enums;
using Enrich.Core;
using Enrich.Core.Configs;
using Enrich.Core.Connection;
using Enrich.Core.Container;
using Enrich.Core.Infrastructure;
using Enrich.Core.Utils;
using Enrich.Dto;
using Enrich.IMS.Infrastructure.Data.Implement.Repositories;
using Enrich.IMS.Services.Implement.Library;
using Enrich.IMS.Services.Implement.Mappers;
using Enrich.IMS.Services.Implement.Services;
using Enrich.IMS.Services.Implement.Validation;
using Enrich.Infrastructure.Cache;
using Enrich.Infrastructure.Data;
using Enrich.Infrastructure.Data.Dapper;
using Enrich.Infrastructure.Email;
using Enrich.Infrastructure.IoC;
using Enrich.Infrastructure.Log;
using Enrich.Infrastructure.Slack;
using Enrich.Infrastructure.SMS;
using Enrich.Payment.MxMerchant;
using Enrich.Payment.MxMerchant.Api;
using Enrich.RestApi.NetCorePlatform;
using Enrich.RestApi.NetCorePlatform.Auth;
using Enrich.RestApi.NetCorePlatform.Auth.Basic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;

namespace Enrich.Base.RestApi
{
    public class EnrichAutofacRegister : IEnrichStartup
    {
        private static readonly IMemoryCache _memoryCacheInstance = new Microsoft.Extensions.Caching.Memory.MemoryCache(new MemoryCacheOptions());

        public int Order => 1;
     

        public void ConfigureServices(ContainerBuilder builder, IConfiguration configuration)
        {
            Register(builder, configuration);
        }

        public void Configure(Microsoft.AspNetCore.Builder.IApplicationBuilder application)
        {
            application.UseAuthentication();
            application.UseRouting();
        }

        #region method
        private void Register(ContainerBuilder builder, IConfiguration config)
        {
            builder.RegisterType<EnrichContext>().InstancePerLifetimeScope();
            builder.RegisterType<CustomerTransactionService>().InstancePerLifetimeScope();
            builder.RegisterType<ActivatorService>().InstancePerLifetimeScope();
            builder.Register(c => new HttpClient()).As<HttpClient>();
            RegisterEnrichUtils(builder, config);
            RegisterHttpContexts(builder);
            RegisterRepositories(builder, config);
            RegisterServices(builder);
            RegisterMappers(builder);
            RegisterValidations(builder);
            RegisterAuth(builder);
            RegisterCache(builder, config);
            RegisterConfigs(builder, config);
            builder.LoadConfig(config);
        }

        private void RegisterCache(ContainerBuilder builder, IConfiguration config)
        {
            builder.RegisterInstance(_memoryCacheInstance).As<IMemoryCache>().SingleInstance();
        }

        private static void RegisterHttpContexts(ContainerBuilder builder)
        {
            builder.RegisterType<DefaultEnrichContextFactory>().As<IEnrichContextFactory>().SingleInstance();
           // builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>().SingleInstance();
            builder.RegisterType<EnrichContext>().OnRelease(instance => instance.Release()).InstancePerLifetimeScope();
        }

        private static void RegisterEnrichUtils(ContainerBuilder builder, IConfiguration config)
        {
            builder.RegisterType<AutofacContainer>().As<IEnrichContainer>().InstancePerLifetimeScope();

            // log: dont use SingleInstance -> use EnrichContext
            builder.RegisterType<SerilogImp>().As<IEnrichLog>().OnRelease(instance => instance.Release()).InstancePerLifetimeScope();
           // builder.RegisterType<EnrichSms>().As<IEnrichSms>().InstancePerLifetimeScope();
            builder.RegisterType<EnrichSendEmail>().As<IEnrichSendEmail>().InstancePerLifetimeScope();

            builder.RegisterType<NoCache>().Keyed<IEnrichCache>(Constants.CacheName.NoCache).PreserveExistingDefaults().SingleInstance();
            builder.RegisterType<Infrastructure.Cache.MemoryCache>().Keyed<IEnrichCache>(Constants.CacheName.MemoryCache).SingleInstance();
            builder.RegisterType<RedisCache>().Keyed<IEnrichCache>(Constants.CacheName.RedisCache)
                    .WithParameter("configuration", config["Caches:Settings:Redis:ConnectionString"])
                    .WithParameter("database", config["Caches:Settings:Redis:Database"])
                    .WithParameter("cacheNamespace", config["Caches:Settings:Redis:Namespace"])
                    .WithParameter(new ResolvedParameter((pi, ctx) => pi.ParameterType == typeof(IEnrichCache) && pi.Name == "backupCache", (pi, ctx) =>
                    {
                        var backupName = config["Caches:Settings:Redis:BackupCacheName"];
                        return !string.IsNullOrWhiteSpace(backupName) && ctx.TryResolveKeyed(backupName, typeof(IEnrichCache), out var service) ? (IEnrichCache)service : null;
                    })
                    )
                    .SingleInstance();
        }

        private static void RegisterValidations(ContainerBuilder builder)
        {
            EnrichValidationHelper.GetValidatorTypes().ForEach(type => builder.RegisterType(type).InstancePerLifetimeScope()); //.LogCall(logCall)
        }

        private static void RegisterAuth(ContainerBuilder builder)
        {
            builder.RegisterType<BasicAuthProvider>().Keyed<IAuthProvider>(AuthMode.Basic);

            //OAuth2          
            builder.RegisterType<OAuthAuthProvider>().Keyed<IAuthProvider>(AuthMode.OAuth);
        }

        private static void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterType<EnrichSlack>().As<IEnrichSlack>().WithParameter("token", "xoxb-3408625377281-3638852660820-JO64QRhzrsc8EEf243f3UJve").InstancePerLifetimeScope();
            builder.RegisterAssemblyTypes(Assembly<TicketService>()).As(t => GetInterfaceType(t, "IGenericService", "IService"));

            builder.RegisterType<PaymentFunction>().As<IPaymentFunction>().InstancePerLifetimeScope();
            builder.RegisterType<MxMerchantFunction>().As<IMxMerchantFunction>().InstancePerLifetimeScope();
            builder.RegisterType<OAuthNew>().As<IOAuthNew>().InstancePerLifetimeScope();
            builder.RegisterType<OAuthUtilities>().As<IOAuthUtilities>().InstancePerLifetimeScope();
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

        private static void RegisterMappers(ContainerBuilder builder)
        {
            //Mappers for IMS
            builder.RegisterAssemblyTypes(Assembly<MemberMapper>())
                .Where(t => t.Name.EndsWith("Mapper"))
                .AsImplementedInterfaces()
                .SingleInstance();
        }

        private static void RegisterConfigs(ContainerBuilder builder, IConfiguration config)
        {
            //app-settings
            builder.RegisterInstance(config).SingleInstance();
            builder.RegisterType<EnrichAppConfig>().As<IAppConfig>();
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

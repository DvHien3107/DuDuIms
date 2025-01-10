using Autofac;
using Autofac.Extensions.DependencyInjection;
using Enrich.Dto.Base;
using Enrich.IMS.SubscriptionRecurring.Worker;
using Enrich.Infrastructure.Log;
using Enrich.RestApi.NetCorePlatform.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Enrich.IMS.SubscriptionRecurring
{
    public class Program
    {
        private static readonly string HostName = Environment.GetEnvironmentVariable("HOST_HOSTNAME");
        public static IConfiguration Configuration { get; set; }
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    Configuration = hostContext.Configuration;
                    services.AddHostedService<SubscriptionRecurring.Worker.RecurringWorker>();
                    services.AddHttpClient();
                }).UseServiceProviderFactory(new AutofacServiceProviderFactory())
                    .ConfigureContainer<ContainerBuilder>(builder =>
                    {
                        builder.ConfigureAutofacBaseServices(Configuration);
                    })
                .UseWindowsService()
                .ConfigureLogging(ConfigureLogging);

        private static void ConfigureLogging(HostBuilderContext context, ILoggingBuilder builder)
        {
            var _logSetting = context.Configuration.GetSection("Logs").Get<LogSetting>();
            Log.Logger = SerilogHelper.CreateLogger(_logSetting);
        }
    }
}
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Enrich.Dto.Base;
using Enrich.IMS.PaymentLaterNotification.Worker;
using Enrich.Infrastructure.Log;
using Enrich.RestApi.NetCorePlatform.Infrastructure;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Enrich.IMS.SubscriptionRecurring
{
    public class Program
    {
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
                    services.AddHostedService<PaymentLaterNotificationWorker>();
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
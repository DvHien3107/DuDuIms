using Enrich.Common;
using Enrich.Core;
using Enrich.Dto;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.IMS.Services.Interface.Services;
using Enrich.RestApi.NetCorePlatform;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Enrich.IMS.SubscriptionRecurring.Worker
{
    public class RecurringWorker : BackgroundService
    {
        private readonly IEnrichLog _logger;
        private readonly IRecurringPlanningService _serviceRecurringPlanning;
        private readonly ISystemConfigurationRepository _repositorySystem;
        private Timer _timer;

        public RecurringWorker(
            EnrichContext context,
            IEnrichContextFactory contextFactory,
            IEnrichLog logger,
            IRecurringPlanningService serviceRecurringPlanning,
            ISystemConfigurationRepository repositorySystem)
        {
            _logger = logger;
            _serviceRecurringPlanning = serviceRecurringPlanning;
            _repositorySystem = repositorySystem;

            contextFactory.Populate(context, new AuthRawData { FullName = Constants.ServiceName.Recurring, UserName = Constants.SystemName });
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var timeNow = DateTime.UtcNow;
            var timeConfig = (_repositorySystem.GetConfigurationRecuringTime() ?? "00:00").Split(':');
            var timeRun = new DateTime(timeNow.Year, timeNow.Month, timeNow.Day, int.Parse(timeConfig[0]), int.Parse(timeConfig[1]), int.Parse(timeConfig[2]));
            #if DEBUG
            timeNow = DateTime.Now;
            timeRun = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute + 1, DateTime.Now.Second);
            #endif
            var scheduler = (timeRun - timeNow).Duration();
            if (timeNow > timeRun)
            {
                timeRun = timeRun.AddDays(1);
                scheduler = (timeNow - timeRun).Duration();
            }
            _logger.Info($"Execute recurring service, next run affter {scheduler.TotalMinutes} minutes");
            _timer = new Timer(async (state) =>
            {
                var guid = Guid.NewGuid().ToString();
                var stopWatch = Stopwatch.StartNew();
                var memory = 0.0;
                _logger.Info($"{guid} Start recurring at {DateTime.UtcNow.ToString("MMM dd, yyyy hh:mm:ss")} utc");
                using (Process proc = Process.GetCurrentProcess())
                {
                    await _serviceRecurringPlanning.RecurringSubscriptionAsync();
                    stopWatch.Stop();
                    // The proc.PrivateMemorySize64 will returns the private memory usage in byte.
                    // Would like to Convert it to Megabyte? divide it by 2^20
                    memory = proc.PrivateMemorySize64 / (1024 * 1024);
                    _logger.Info($"{guid} Complete recurring {DateTime.UtcNow.ToString("MMM dd, yyyy")}, memory usage {memory}Mb in {stopWatch.Elapsed.TotalSeconds}s");
                }
            }, null, scheduler, TimeSpan.FromDays(1));

        }
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.Info($"Recurring service has been stopped");
            await base.StopAsync(cancellationToken);
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.Info($"Recurring service has been started");
            await base.StartAsync(cancellationToken);
        }

    }
}

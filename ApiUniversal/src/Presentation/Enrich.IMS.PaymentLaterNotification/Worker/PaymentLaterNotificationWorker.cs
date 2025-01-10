using Enrich.BusinessEvents.IMS;
using Enrich.Common;
using Enrich.Common.Enums;
using Enrich.Common.Helpers;
using Enrich.Core;
using Enrich.Core.Container;
using Enrich.Dto;
using Enrich.IMS.Dto;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.IMS.Services.Interface.Services;
using Enrich.RestApi.NetCorePlatform;
using System.Diagnostics;

namespace Enrich.IMS.PaymentLaterNotification.Worker;

public class PaymentLaterNotificationWorker : BackgroundService
{
    private readonly IEnrichLog _logger;
    private readonly IEnrichContainer _container;
    private readonly IOptionConfigRepository _configRepository;
    private readonly int _delayMinuteStart;
    private Timer _timer;

    public PaymentLaterNotificationWorker(
        IConfiguration appConfig,
        IEnrichLog logger,
        EnrichContext context,
        IEnrichContainer container,
        IEnrichContextFactory contextFactory,
        IOptionConfigRepository configRepository)
    {
        _logger = logger;
        _container = container;
        _configRepository = configRepository;
        _delayMinuteStart = appConfig.GetSection("DelayMinute").Get<int>();

        contextFactory.Populate(context, new AuthRawData { FullName = Constants.ServiceName.PaymentLaterNotification, UserName = Constants.SystemName });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var timeNow = TimeHelper.GetUTCNow();
            var configScanTime = await _configRepository.GetConfigAsync(ConfigEnum.Key.autoScanTime);
            if (configScanTime == null) throw new Exception(ValidationMessages.NotFound.ConfigPeriodScan);
            var arrScanTime = configScanTime.Value.Split(':');
            var timeRun = new DateTime(timeNow.Year, timeNow.Month, timeNow.Day, int.Parse(arrScanTime[0]), int.Parse(arrScanTime[1]), int.Parse(arrScanTime[2])).AddMinutes(_delayMinuteStart);
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

            _logger.Info($"[{Constants.ServiceName.PaymentLaterNotification}] Next run affter {scheduler.TotalMinutes} minutes, delay {_delayMinuteStart} minutes");
            _timer = new Timer(async (state) =>
            {
                var timeShow = TimeHelper.GetUTCNow();
                var guid = Guid.NewGuid().ToString("N");
                var stopWatch = Stopwatch.StartNew();
                var memory = 0.0;
                _logger.Info($"{guid} Start scan payment later at {timeShow.ToString(Constants.Format.Date_ddMMyyyy_HHmmss)} utc");
                using (Process proc = Process.GetCurrentProcess())
                {
                    try
                    {
                        await PaymentLaterNotificationAsync(guid);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, $"{guid} Error scan payment later at {timeShow.ToString(Constants.Format.Date_ddMMyyyy_HHmmss)} utc");
                    }

                    stopWatch.Stop();
                    memory = proc.PrivateMemorySize64 / (1024 * 1024);
                    _logger.Info($"{guid} Complete scan payment later at {timeShow.ToString(Constants.Format.Date_ddMMyyyy_HHmmss)} utc, memory usage {memory}Mb in {stopWatch.Elapsed.TotalSeconds}s");
                }

            }, null, scheduler, TimeSpan.FromDays(1));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Error start service.");
        }
    }

    private async Task PaymentLaterNotificationAsync(string sessionId)
    {
        var service = _container.Resolve<IOrderEventService>();
        if (service == null) return;

        var request = new BusinessEventRequest { ExtendConditions = BusinessEventEnum.ExtendCondition.Waiting(BusinessEventEnum.Event.OrderPay.Later) };
        var orderPaymentLater = await service.GetBusinessEventAsync<OrderPaymentLaterEvent>(request);

        if (orderPaymentLater == null || orderPaymentLater.Count() == 0)
            return;

        await service.SendNotificationPaymentLaterAsync(orderPaymentLater, sessionId);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.Info($"[{Constants.ServiceName.PaymentLaterNotification}] Service has been stopped");
        await base.StopAsync(cancellationToken);
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.Info($"[{Constants.ServiceName.PaymentLaterNotification}] Service has been started");
        await base.StartAsync(cancellationToken);
    }
}
using Enrich.Core.Infrastructure;
using Enrich.DataTransfer;
using Enrich.IServices;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.AppLB.OptionConfig;
using EnrichcousBackOffice.Areas.AutoServices.Services;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Models.CustomizeModel;
using EnrichcousBackOffice.Services;
using EnrichcousBackOffice.Utils.OptionConfig;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EnrichcousBackOffice.Areas.AutoServices.Controllers
{
    public class StartController : Controller
    {
        // GET: AutoServices/Start
      [Authorize(Roles = "admin")]
        public ActionResult Index()
        {
            var curMem = Authority.GetCurrentMember();
            var _optionConfigurationService = new OptionConfigService();
            var config = _optionConfigurationService.LoadSetting<Config>();
            var Now = DateTime.UtcNow;
            var ScanTime = new DateTime(Now.Year, Now.Month, Now.Day,config.AutoScan_Time.Value.Hours, config.AutoScan_Time.Value.Minutes, config.AutoScan_Time.Value.Seconds);
            ViewBag.ScanTime = string.Format("{0:r}", ScanTime);
            ViewBag.mem = curMem;
            return View();
            //}
        }
        [Authorize(Roles = "admin")]
        public ActionResult RestartScan()
        {
            try
            {
                SetRecurringExecuteJobService.RestartRecurringAutoScan();
                TempData["s"] = "Restart Scan Success";
                WriteLogErrorService _writeLogErrorService = new WriteLogErrorService();
                IMSLog logStartApplication = new IMSLog()
                {
                    Id = Guid.NewGuid().ToString(),
                    CreateBy = "IMS System",
                    CreateOn = DateTime.UtcNow,
                    StatusCode = 200,
                    Success = true,
                    Description = "Restart AutoScan Success",
                };
                _writeLogErrorService.InsertLogError(logStartApplication);
                return Json(new { status = true });
            }
            catch(Exception ex)
            {
                return Json(new { status = false,message = ex.Message });
            }
        }
        public async Task<JsonResult> StartScan(bool admin_scan = false)
        {
            TaskReminderService.Scan_Task();

            var slog = await new RecurringService().autoRecurringScan();
            slog.Add(new scanlog { time = DateTime.UtcNow, log = "Scan completed with <b>" + slog.Count(s => !string.IsNullOrEmpty(s.ex)) + " errors</b>" });
            string CreateBy = "";
            if (admin_scan)
            {
                P_Member curMem = Authority.GetCurrentMember();
                CreateBy = curMem.FullName;
            }
            else
            {
                CreateBy = "IMS System";
            }
            using (var db = new WebDataModel())
            {
                IMSLog imslog = new IMSLog()
                {
                    Id = Guid.NewGuid().ToString(),
                    CreateBy = CreateBy,
                    CreateOn = DateTime.UtcNow,
                    Url = "/Start/StartScan",
                    RequestUrl = "/Start/StartScan",
                    StatusCode = 200,
                    Success = true,
                    RequestMethod = "Get",
                    Description = "AutoScanSystem",
                    JsonRespone = JsonConvert.SerializeObject(slog),
                };
                db.IMSLogs.Add(imslog);
                db.SaveChanges();
            }
            return Json(new object[] { true, "Scan completed!" });
        }
        public ActionResult getLogsScan(DateTime? FromDate, DateTime? ToDate)
        {
            var from = (FromDate ?? DateTime.UtcNow).Date.AddDays(-4);
            var to = (FromDate ?? DateTime.UtcNow).Date.AddDays(1);
            using (var db = new WebDataModel())
            {
                var logs = db.IMSLogs.Where(l => l.Description == "AutoScanSystem" && l.CreateOn > from && l.CreateOn < to).AsEnumerable().Select(l =>
                new IMSLog_scan
                {
                    scanlogs = JsonConvert.DeserializeObject<List<scanlog>>(l.JsonRespone),
                    ScanBy = l.CreateBy,
                    CreateAt = l.CreateOn,
                }).ToList();
                return PartialView("~\\Areas\\AutoServices\\Views\\Start\\_ScanLogs.cshtml", logs);
            }
        }
        public string GetClientIP()
        {
            string visitorIPAddress = HttpContext.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (String.IsNullOrEmpty(visitorIPAddress))
                visitorIPAddress = HttpContext.Request.ServerVariables["REMOTE_ADDR"];

            if (string.IsNullOrEmpty(visitorIPAddress))
                visitorIPAddress = HttpContext.Request.UserHostAddress;

            return visitorIPAddress;
        }
        #region with hangfire
        public async Task CallStart()
        {
            var _optionConfigurationService = new OptionConfigService();
            var config = _optionConfigurationService.LoadSetting<Config>();
            var _context = EngineContext.Current.Resolve<EnrichContext>();
            _context.ApplicationName = "HangFire";
            if (config.AutoScan_Enable == true)
            {
                await this.StartScan();
            }
        }

        public async Task CallStartSendNotyRenew()
        {
            var _logService = EngineContext.Current.Resolve<ILogService>();
            try
            {
                _logService.Info($"[SendAutoRenewNotification] start send auto renew notification");
                var _optionConfigurationService = new OptionConfigService();
                var config = _optionConfigurationService.LoadSetting<Config>();
                var _context = EngineContext.Current.Resolve<EnrichContext>();
                _context.ApplicationName = "HangFire";
                if (config.AutoScan_Enable == true)
                {
                    await new RecurringService().SendNotyAutoRenew();
                }
                _logService.Info($"[SendAutoRenewNotification] send auto renew notification success");
            }
            catch (Exception ex)
            {
                _logService.Error(ex,$"[SendAutoRenewNotification] Send auto renew notification failed");
            }
        }

        #endregion
    }
}
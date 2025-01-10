using EnrichcousBackOffice.AppLB.OptionConfig;
using EnrichcousBackOffice.Areas.AutoServices.Controllers;
using EnrichcousBackOffice.Utils.OptionConfig;
using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;

namespace EnrichcousBackOffice.Services
{
    public static class SetRecurringExecuteJobService
    {
        public static void CallJobRecurringAutoScan()
        {
            var _optionConfigurationService = new OptionConfigService();
            var config = _optionConfigurationService.LoadSetting<Config>();
            StartController startController = new StartController();
            string cronExp = config.AutoScan_Time.Value.Minutes+" " + config.AutoScan_Time.Value.Hours+ " * * *";
            RecurringJob.AddOrUpdate("RecurringAutoScan", () => startController.CallStart(), cronExp);

            // send notice at 11h eastern time (-05) (16h Utc)
            string cronExpExpires = "0 16 * * *";
            RecurringJob.AddOrUpdate("SendNotyAutoRenew", () => startController.CallStartSendNotyRenew(), cronExpExpires);
            // scan send auto renewal to merchant

        }
        public static void RestartRecurringAutoScan()
        {
            var _optionConfigurationService = new OptionConfigService();
            var config = _optionConfigurationService.LoadSetting<Config>();
            StartController startController = new StartController();
            string cronExp = config.AutoScan_Time.Value.Minutes + " " + config.AutoScan_Time.Value.Hours + " * * *";
            RecurringJob.AddOrUpdate("RecurringAutoScan", () => startController.CallStart(), cronExp);
        }
    }
    public class ForceSessionModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.PostAuthorizeRequest += OnPostAuthorizeRequest;
        }

        public void Dispose() { }

        private void OnPostAuthorizeRequest(object sender, EventArgs eventArgs)
        {
            var context = ((HttpApplication)sender).Context;
            var request = context.Request;
            if ((request != null
                 && request.AppRelativeCurrentExecutionFilePath != null
                 && request.AppRelativeCurrentExecutionFilePath.StartsWith("~/hangfire", StringComparison.InvariantCultureIgnoreCase)))
            {
                context.SetSessionStateBehavior(SessionStateBehavior.Required);
            }
        }
    }
}
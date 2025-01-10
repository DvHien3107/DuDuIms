using Enrich.Common;
using Enrich.Core;
using Enrich.Dto;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.IMS.Services.Interface.Services;
using Enrich.RestApi.NetCorePlatform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Enrich.IMS.ApiService.Controllers
{
    [AllowAnonymous]
    [Route("[controller]")]
    public class RecurringController : Controller
    {
        private readonly IRecurringPlanningService _serviceRecurringPlanning;

        public RecurringController(
            EnrichContext context,
            IEnrichContextFactory contextFactory,
            IRecurringPlanningService serviceRecurringPlanning)
        {
            _serviceRecurringPlanning = serviceRecurringPlanning;
            contextFactory.Populate(context, new AuthRawData { FullName = Constants.ServiceName.Recurring, UserName = Constants.SystemName });
        }
        [HttpGet(Name = "SubscriptionRecurring")]
        public async Task<JsonResult> SubscriptionRecurring()
        {
            try
            {
                string result = await _serviceRecurringPlanning.RecurringSubscriptionAsync_NextGen();
                await _serviceRecurringPlanning.DoProc("exec P_RecuringLicense");
                return Json(new { status = 200, result });

            }
            catch (Exception ex) {
                return Json(new { status = 500, Mess = ex.Message, Data = ex });

            }
        }



    }
}

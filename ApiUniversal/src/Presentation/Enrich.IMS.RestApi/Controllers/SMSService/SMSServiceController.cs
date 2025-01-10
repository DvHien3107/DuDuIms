using Enrich.Common.Enums;
using Enrich.Core;
using Enrich.Core.Container;
using Enrich.Core.Utils;
using Enrich.Dto;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.EnrichSMSService;
using Enrich.IMS.Dto.StoreBaseService;
using Enrich.IMS.Entities;
using Enrich.IMS.RestApi.Library;
using Enrich.IMS.Services.Interface.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Enrich.IMS.RestApi.Controllers
{
    [EnrichRoute(EnrichRouteName.SMSService)]
    public class SMSServiceController : EnrichAuthApiController
    {
        private readonly IStoreBaseServiceService _serviceStoreBaseService;
        private readonly IEnrichSMSService _enrichSMSService;

        public SMSServiceController(
            EnrichContext context,
            IEnrichContainer container,
            IStoreBaseServiceService serviceStoreBaseService,
            IEnrichSMSService enrichSMSService) : base(context, container)
        {
            _serviceStoreBaseService = serviceStoreBaseService;
            _enrichSMSService = enrichSMSService;
        }


        [HttpGet("{store}/SyncToltalRemainingSMS/{totalnumber}")]
        public async Task<ActionResult<BaseServiceRemainingResponse>> ToltalSMSRemainingValidationAsync(string store, int totalnumber)
        {
            if (string.IsNullOrEmpty(store))
            {
                return BadRequest();
            }

            return Ok(await _serviceStoreBaseService.SyncRemainingValidationAsync(store, totalnumber, BaseServiceEnum.Code.SMS));
        }


        [HttpPost("history/timeline")]
        public async Task<ActionResult<HistoryTimeLineResponse>> GetTimeLineAsync(HistoryTimeLineRequest request)
        {
            var response = await _enrichSMSService.TimeLineHistoryAsync(request);

            return Paging(response);
        }
    }
}
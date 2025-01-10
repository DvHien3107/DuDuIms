using Enrich.Core;
using Enrich.Dto.Base.Requests;
using Enrich.IMS.Entities;
using Enrich.IMS.Services.Interface.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Enrich.IMS.RestApi.Library;
using Enrich.IMS.Dto;
using Enrich.Dto;
using Enrich.Core.Container;
using Enrich.IMS.Dto.OrderSubscription;
using Enrich.IMS.Dto.Enums;
using Enrich.Common.Helpers;
using Enrich.Common.Enums;

namespace Enrich.IMS.RestApi.Controllers
{
    [EnrichRoute(EnrichRouteName.OrderSubscrition)]
    public class OrderSubscriptionController : EnrichAuthApiController<OrderSubscription, OrderSubscriptionDto>
    {
        private readonly IEnrichLog _enrichLog;
        private readonly IOrderSubscriptionService _service;
        public OrderSubscriptionController(IEnrichLog enrichLog, IOrderSubscriptionService service, EnrichContext context, IEnrichContainer container)
            : base(service, context, container)
        {
            _enrichLog = enrichLog;
            _service = service;
        }

        #region Search

        [HttpPost("transaction")]
        public async Task<ActionResult<OrderSubscriptionSearchResponse>> SearchAsync(OrderSubscriptionSearchRequest request)
        {
            var response = await _service.SearchAsyncByProc(request);
            return Paging(response);
        }

        [HttpPost("addon/{package}")]
        public async Task<ActionResult<SubscriptionPackageSearchResponse>> SearchSubscriptionPackageAsync([FromRoute]string package,[FromBody] SubscriptionPackageSearchRequest request)
        {
            if (string.IsNullOrEmpty(package) || !package.TryGetEnum<BaseServiceEnum.Code>(out var epackage))
            {
                return BadRequest();
            }

            var response = await _service.SearchSubscriptionPackageAsync(package, request);
            return Ok(response);
        }
        #endregion
    }
}
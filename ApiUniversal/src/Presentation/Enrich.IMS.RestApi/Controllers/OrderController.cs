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
using Microsoft.AspNetCore.JsonPatch;
using Enrich.Common.Enums;
using System.Collections.Generic;
using System.Linq;
using Enrich.IMS.Infrastructure.Data.Interface.Builders;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Microsoft.AspNetCore.Http;

namespace Enrich.IMS.RestApi.Controllers
{
    [EnrichRoute(EnrichRouteName.Order)]
    public class OrderController : EnrichAuthApiController<Order, OrderDto>
    {
        private readonly IEnrichLog _enrichLog;
        private readonly IOrderService _service;
        public OrderController(IEnrichLog enrichLog, IOrderService service, EnrichContext context, IEnrichContainer container)
            : base(service, context, container)
        {
            _enrichLog = enrichLog;
            _service = service;
        }

        public override async Task<ActionResult> CreateAsync([FromBody] OrderDto request)
        {
            return StatusCode(StatusCodes.Status201Created, new { Message = "" });
        }
    }
}


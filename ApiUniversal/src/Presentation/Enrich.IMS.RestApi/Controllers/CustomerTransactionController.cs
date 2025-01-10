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
using Enrich.IMS.Dto.CustomerTransaction;

namespace Enrich.IMS.RestApi.Controllers
{
    [EnrichRoute(EnrichRouteName.CustomerTransaction)]
    public class CustomerTransactionController : EnrichAuthApiController<CustomerTransaction, CustomerTransactionDto>
    {
        private readonly IEnrichLog _enrichLog;
        private readonly ICustomerTransactionService _service;
        public CustomerTransactionController(IEnrichLog enrichLog, ICustomerTransactionService service, EnrichContext context, IEnrichContainer container)
            : base(service, context, container)
        {
            _enrichLog = enrichLog;
            _service = service;
        }

        #region Search

        [HttpPost("search")]
        public async Task<ActionResult<CustomerTransactionSearchResponse>> SearchAsync(CustomerTransactionSearchRequest request)
        {
            var response = await _service.SearchAsync(request);
            return Paging(response);
        }
        #endregion
    }
}


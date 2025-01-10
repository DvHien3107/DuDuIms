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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Enrich.IMS.RestApi.Controllers
{
    [EnrichRoute(EnrichRouteName.MondayConnector)]
    public class MondayConnectorController : EnrichAuthApiController
    {
        private readonly IMondayConnector _mondayConnector;
        public MondayConnectorController(
            EnrichContext context,
            IEnrichContainer container,
            IMondayConnector mondayConnector) : base(context, container)
        {
            _mondayConnector = mondayConnector;
        }


        [HttpGet("sync-saleslead")]
        public async Task<ActionResult> SyncingSalesLeadAsync()
        {
            var result = await _mondayConnector.SyncingSalesLeadAsync();
            if (result) return StatusCode(StatusCodes.Status200OK);
            return StatusCode(StatusCodes.Status204NoContent);
        }
    }
}
using Enrich.Common.Helpers;
using Enrich.Core.Container;
using Enrich.Dto;
using Enrich.Dto.Base;
using Enrich.Dto.Base.Exceptions;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.Common;
using Enrich.IMS.Dto.TicketFeedback;
using Enrich.IMS.Entities;
using Enrich.IMS.RestApi.Library;
using Enrich.IMS.RestApi.Library.Filters;
using Enrich.IMS.Services.Interface.Mappers;
using Enrich.IMS.Services.Interface.Services;
using Enrich.Services.Implement.Library;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Enrich.IMS.RestApi.Controllers
{
    [EnrichRoute(EnrichRouteName.TicketFeedback)]
    public class TicketFeedbackController : EnrichAuthApiController<TicketFeedback, TicketFeedbackDto>
    {
        private ITicketFeedbackService _service => _genericService as ITicketFeedbackService;

        public TicketFeedbackController(
            ITicketFeedbackService service,
            EnrichContext context,
            IEnrichContainer container
            ) : base(service, context, container)
        {
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetAsync(long id)
        {
            var ticket = await _service.GetByIdAsync(id);
            return Ok(ticket);
        }

        [HttpPost("new/{ticketId}")]
        public async Task<ActionResult> InsertAsync([FromRoute] long ticketId, [FromBody] TicketFeedbackDto request)
        {
            if (ticketId <= 0)
            {
                return BadRequest();
            }

            request.TicketId = ticketId;
            var response = await _service.CreateAsync(request);
            return StatusCode(StatusCodes.Status201Created, response);
        }

        [HttpPatch("edit/{id}")]
        public async Task<ActionResult> UpdateCommentByPatchAsync(long id, [FromBody][EnrichNotNull] List<Operation<TicketFeedbackDto>> operations)
        {
            var loadOption = GetLoadOption<TicketFeedbackDto, TicketFeedbackUpdateOption>(operations);

            //alway set true be customer beyond saleslead
            var ticketFeedback = await _service.GetByIdAsync(id);
            if (ticketFeedback == null)
            {
                return ExceptionNotFound(ExceptionCodes.SaleLeadComment_NotFound, $@"Ticket feedback id {id} not found");
            }

            var changes = GetJsonPatchOperationsAsDto(operations);
            var oldFeedback = ticketFeedback.CloneByJson();

            //merge current with data patch
            JsonPatchApplyTo(ticketFeedback, operations);

            await _service.UpdateAsync(new TicketFeedbackUpdateRequest(ticketFeedback, oldFeedback)
            {
                PatchChanges = changes,
                UpdateOption = loadOption
            });

            return StatusOk();
        }
    }
}
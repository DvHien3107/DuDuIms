using Enrich.Common;
using Enrich.Core.Container;
using Enrich.Core.Utils;
using Enrich.Dto;
using Enrich.Dto.Base.Exceptions;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.Ticket;
using Enrich.IMS.Dto.Ticket.Queries;
using Enrich.IMS.Dto.Ticket.Reponses;
using Enrich.IMS.Entities;
using Enrich.IMS.RestApi.Library;
using Enrich.IMS.RestApi.Library.Filters;
using Enrich.IMS.Services.Interface.Services;
using Enrich.Services.Implement.Library;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.RestApi.Controllers
{
    [EnrichRoute(EnrichRouteName.Ticket)]
    public class TicketController : EnrichAuthApiController<Ticket, TicketDto>
    {
        #region Fields
        private ITicketService _service => _genericService as ITicketService;
        private readonly IProjectMilestoneService _projectMilestoneService;
        private readonly ITicketStatusService _ticketStatusService;
        private readonly ITicketTypeService _ticketTypeService;
        private readonly IEnrichCache _cache;
        #endregion

        #region Ctors
        public TicketController(ITicketService service, EnrichContext context, IEnrichContainer container, IProjectMilestoneService projectMilestoneService, ITicketStatusService ticketStatusService, ITicketTypeService ticketTypeService)
           : base(service, context, container)
        {
            _cache = container.ResolveByKeyed<IEnrichCache>(Constants.CacheName.RedisCache);
            _projectMilestoneService = projectMilestoneService;
            _ticketStatusService = ticketStatusService;
            _ticketTypeService = ticketTypeService;
        }
        #endregion

        #region Tickets
        /// <summary>
        /// Get ticket detail
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ActionResult> GetTicketDetailAsync(long id)
        {
            var ticket = await _service.GetDetailTicketAsync(id);
            return Ok(ticket);
        }

        /// <summary>
        /// Update ticket
        /// </summary>
        /// <param name="id"></param>
        /// <param name="operations"></param>
        /// <returns></returns>
        [HttpPatch("{id}")]
        public async Task<ActionResult> UpdateTicketByPatchAsync(long id, [FromBody][EnrichNotNull] List<Operation<TicketDto>> operations)
        {
            var loadOption = GetLoadOption<TicketDto, TicketUpdateOption>(operations);
            var ticket = await _service.GetDetailTicketAsync(id);
            if (ticket == null)
            {
                return ExceptionNotFound(ExceptionCodes.Ticket_NotFound, $@"Ticket id {id} not found");
            }
            var changes = GetJsonPatchOperationsAsDto(operations);
            var oldTicket= ticket.CloneByJson();

            //merge current with data patch
            JsonPatchApplyTo(ticket, operations);

            await _service.UpdateTicketAsync(new TicketUpdateRequest(ticket, oldTicket)
            {
                PatchChanges = changes,
                UpdateOption = loadOption
            });

            return StatusOk();
        }
        #endregion

        #region Jira Connector Intergrate
        /// <summary>
        /// Create jira issue from ticket id
        /// </summary>
        /// <param name="requestDto"></param>
        /// <returns></returns>
        [HttpPost("jira-issue-create")]
        public async Task<ActionResult> CreateJiraIssue(CreateOrUpdateJiraIssueRequestDto requestDto)
        {
            try
            {
                await _service.CreateJiraIssueFromTicket(requestDto.TicketId);
                return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest();
            }
        }
        #endregion

        #region Ticket/Project
        /// <summary>
        /// Get All Project
        /// </summary>
        /// <returns></returns>
        [HttpGet("projects")]
        public async Task<ActionResult> GetAllProject()
        {
            try
            {
               var projects =  await _projectMilestoneService.GetAllProjectAsync();
                return StatusCode(StatusCodes.Status200OK,projects);
            }
            catch (Exception ex)
            {
                return StatusBadRequest();
            }
        }

        /// <summary>
        /// Get project by id
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        [HttpGet("project/{projectId}")]
        public async Task<ActionResult> GetAllProject(string projectId)
        {
            try
            {
                var project = await _projectMilestoneService.GetAsync(projectId);
                return StatusCode(StatusCodes.Status200OK, project);
            }
            catch (Exception ex)
            {
                return StatusBadRequest();
            }
        }

        /// <summary>
        /// get ticket status by project id
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        
        [HttpGet("project/{projectId}/statuses")]
        public async Task<ActionResult> GetStatusByProject(string projectId)
        {
            try
            {
                var statuses = await _ticketStatusService.GetTicketStatusByProjectAsync(projectId);
                return StatusCode(StatusCodes.Status200OK, statuses);
            }
            catch (Exception ex)
            {
                return StatusBadRequest();
            }
        }

        [HttpGet("project/{projectId}/types")]
        public async Task<ActionResult> GetTypesByProject(string projectId)
        {
            try
            {
                var statuses = await _ticketTypeService.GetTicketTypesByProjectIdAsync(projectId);
                return StatusCode(StatusCodes.Status200OK, statuses);
            }
            catch (Exception ex)
            {
                return StatusBadRequest();
            }
        }
        #endregion

    }
}

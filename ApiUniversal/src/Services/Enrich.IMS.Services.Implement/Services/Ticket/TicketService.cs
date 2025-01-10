using Enrich.Common;
using Enrich.Common.Helpers;
using Enrich.Core.Container;
using Enrich.Core.Utils;
using Enrich.Dto;
using Enrich.Dto.Base;
using Enrich.Dto.Base.Exceptions;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.Ticket;
using Enrich.IMS.Dto.Ticket.Queries;
using Enrich.IMS.Dto.Ticket.Reponses;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Builders;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.IMS.Services.Interface.Mappers;
using Enrich.IMS.Services.Interface.Services;
using Enrich.Infrastructure.Data.Dapper.Library;
using Enrich.Services.Implement;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Services
{
    public partial class TicketService : GenericService<Ticket, TicketDto>, ITicketService
    {
        private ITicketRepository _repository => _repositoryGeneric as ITicketRepository;
        private readonly ITicketBuilder _builder;
        private ITicketMapper _mapper => _mapperGeneric as ITicketMapper;
        private readonly IEnrichContainer _container;
        private readonly EnrichContext _context;
        private readonly IJiraConnectorService _jiraConnectorService;
        private readonly ITicketStatusRepository _ticketStatusRepository;
        private readonly string _IMSHost;
        public TicketService(
            ITicketRepository repository,
            IConfiguration configuration,
            ITicketMapper mapper,
            EnrichContext context,
            IEnrichContainer container,
            ITicketBuilder builder,
            IJiraConnectorService jiraConnectorService,
            ITicketStatusRepository ticketStatusRepository) : base(repository, mapper)
        {
            _context = context;
            _container = container;
            _builder = builder;
            _IMSHost = configuration["IMSHost"];
            _jiraConnectorService = jiraConnectorService;
            _ticketStatusRepository = ticketStatusRepository;
        }

        /// <summary>
        /// Get Detail Ticket Async
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<TicketDto> GetDetailTicketAsync(long ticketId)
        {
            var entity = await _repository.FindByIdAsync(ticketId);
            if (entity == null)
            {
                return null;
            }
            var dto = _mapper.Map<TicketDto>(entity);
            var tasks = new MultipleTasks<TicketDto>(dto);
            tasks.Add(nameof(TicketDto.Status), GetTicketStatusAsync(ticketId));
            tasks.Add(nameof(TicketDto.Types), GetTicketTypesAsync(ticketId));
            tasks.Add(nameof(TicketDto.Attachments), GetTicketAttachmentsAsync(ticketId));
            await tasks.WhenAll();
            return dto;
        }
        private async Task<TicketStatusDto> GetTicketStatusAsync(long ticketId)
        {
            var status = await _repository.GetTicketStatusAsync(ticketId);
            return _mapper.Map<TicketStatusDto>(status);
        }
        private async Task<IEnumerable<TicketTypeDto>> GetTicketTypesAsync(long ticketId)
        {
            var types = await _repository.GetTicketTypesAsync(ticketId);
            return types.Select(x => _mapper.Map<TicketTypeDto>(x));
        }

        private async Task<IEnumerable<TicketAttachmentFileDto>> GetTicketAttachmentsAsync(long ticketId)
        {
            var attachments = await _repository.GetTicketAttachmentsAsync(ticketId);
            if (attachments.Count() == 0)
            {
                return new List<TicketAttachmentFileDto>();
            }
            var attachmentsResponse = new List<TicketAttachmentFileDto>();
            var attachmentsDto = attachments.Select(x => _mapper.Map<TicketAttachmentFileDto>(x));
            foreach (var item in attachmentsDto)
            {
                item.FileUrl = item.FileUrl.Replace("\\", "/");
                attachmentsResponse.Add(new TicketAttachmentFileDto
                {
                    Id = item.Id,
                    FileUrl = $"{_IMSHost}/{item.FileUrl}",
                    FileName = item.FileUrl.Split("/").Last(),

                });
            }
            return attachmentsResponse;
        }

        public async Task<SearchTicketResponse> SearchAsync(SearchTicketRequest request)
        {
            request.SqlQueryParam = _mapper.CreateSearchSqlQueryParameter<TicketListItemDto>(request);

            await OptimizeConditionSearchTicket(request);

            var response = await _repository.SearchAsync(request);

            // format data base on require
            PopulateTicketItems(response);

            return response;
        }


        private void PopulateTicketItems(SearchTicketResponse response)
        {
            if (response == null || response.Records == null) return;

            // customize data after get from responsitory
            //foreach (var person in response.Records)
            //{
            //}
        }

        private async System.Threading.Tasks.Task OptimizeConditionSearchTicket(SearchTicketRequest request)
        {
            // optimize Condition
            if (request.Condition == null) request.Condition = new TicketSearchCondition();

            request.Condition.PopulateCountSummaries = false; // !queryParam.HasPaging() || queryParam.Paging.PageIndex == 1;           

            // OnStringFields
            if (request.Condition.OnStringFields?.Count > 0)
            {
                request.Condition.OnStringFields.RemoveAll(a => string.IsNullOrWhiteSpace(a.Key) || string.IsNullOrWhiteSpace(a.Value));
            }
            if (request.Condition.OnStringFields?.Count > 0)
            {
                // convert dto-name to db-name
                var dbFields = FieldDbHelper.GetFields<TicketDto>();

                request.Condition.OnStringFields = request.Condition.OnStringFields
                    .Select(a => new { FieldDb = dbFields.FirstOrDefault(b => b.Prop.Name.Equals(a.Key)), FieldDto = a })
                    .Where(a => a.FieldDb != null).Select(a => new KeyValueDto<string> { Key = a.FieldDb.Name, Value = a.FieldDto.Value }).ToList();

            }
        }

        /// <summary>
        /// Update an existed saleslead
        /// </summary>
        /// <param name="request">UpdateSalesLeadRequest</param>
        /// <returns>UpdateSalesLeadResponse</returns>
        public async Task<TicketUpdateResponse> UpdateTicketAsync(TicketUpdateRequest request)
        {
            var response = new TicketUpdateResponse();
            await SaveFullTicketAsync(false, request, validateData: true);
            //extend response in here

            return response;
        }

        /// <summary>
        /// Save a salesLead
        /// </summary>
        /// <param name="isNew">Insert or update</param>
        /// <param name="request">UpdateSalesLeadRequest </param>
        /// <param name="validateData">Should validate or not</param>
        /// <returns></returns>
        private async Task SaveFullTicketAsync(bool isNew, TicketUpdateRequest request, bool validateData = true)
        {

            if (validateData)
                await ValidateSaveTicketAsync(isNew, request.NewDto, request.OldDto);
            try
            {
                var response = new TicketUpdateResponse();

                // format data before save
                await OptimizeTicketBeforeSaveAsync(isNew, request);

                //map data from dto to entity
                var ticket = _mapper.Map<Ticket>(request.NewDto);

                // save data
                await _repository.UpdateTicketAsync(isNew, ticket, async (ticketId) =>
                {
                    request.NewDto.Id = ticketId;
                    await UpdateTicketStatusAsync(isNew, ticketId, request.NewDto.Status);
                    await UpdateTicketTypesAsync(isNew, ticketId, request.NewDto.Types.ToList());
                });
            }
            catch (Exception ex)
            {
                ThrowWithInner(ExceptionCodes.SaleLead_SaveError, ex.Message, ex);
            }
        }

        /// <summary>
        /// build ticket before saving it
        /// </summary>
        /// <param name="isNew">insert or update</param>
        /// <param name="newDto">new salesLeadDto</param>
        /// <param name="oldDto">old salesLeadDto</param>
        /// <param name="updateOption">SalesLeadUpdateOption</param>
        /// <returns></returns>
        private async Task OptimizeTicketBeforeSaveAsync(bool isNew, TicketUpdateRequest request)
        {
            // build ticket
            await _builder.BuildForSave(isNew, request.NewDto);
            await PopulateTicketStatusAsync(request);
        }

        private async Task PopulateTicketStatusAsync(TicketUpdateRequest request)
        {
            if (request.NewDto.StatusId != request.OldDto.StatusId && long.TryParse(request.NewDto.StatusId, out long statusId))
            {
                var status = await _ticketStatusRepository.GetByIdAsync(statusId);
                if(status != null)
                {
                    request.NewDto.StatusName = status.Name;
                    request.NewDto.Status = status.Clone();
                }
            }
        }

        /// <summary>
        /// Update ticket status
        /// </summary>
        /// <param name="isNew"></param>
        /// <param name="ticketId"></param>
        /// <param name="ticketStatus"></param>
        /// <returns></returns>
        private async Task UpdateTicketStatusAsync(bool isNew, long ticketId, TicketStatusDto ticketStatus)
        {
            var status = _mapper.Map<TicketStatus>(ticketStatus);
            await _repository.UpdateTicketStatusAsync(ticketId, status, isNew);
        }
        /// <summary>
        /// Update ticket status
        /// </summary>
        /// <param name="isNew"></param>
        /// <param name="ticketId"></param>
        /// <param name="ticketStatus"></param>
        /// <returns></returns>
        private async Task UpdateTicketTypesAsync(bool isNew, long ticketId, List<TicketTypeDto> ticketTypes)
        {
            var types = _mapper.Map<List<TicketType>>(ticketTypes);
            await _repository.UpdateTicketTypesAsync(ticketId, types, isNew);
        }

        public async Task CreateJiraIssueFromTicket(long ticketId)
        {
            await _jiraConnectorService.CreateIssue(ticketId);
        }
        public async Task UpdateJiraIssueFromTicket(long ticketId)
        {
            await _jiraConnectorService.UpdateIssue(ticketId);
        }
    }
}

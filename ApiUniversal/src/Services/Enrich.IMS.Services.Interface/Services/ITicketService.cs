using Enrich.Core.Infrastructure.Services;
using Enrich.IMS.Dto.Ticket;
using Enrich.IMS.Dto.Ticket.Queries;
using Enrich.IMS.Dto.Ticket.Reponses;
using Enrich.IMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Interface.Services
{
    public interface ITicketService : IGenericService<Ticket, TicketDto>
    {
        /// <summary>
        /// Get Detail Ticket Async
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<TicketDto> GetDetailTicketAsync(long id);
        /// <summary>
        /// Search Ticket
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<SearchTicketResponse> SearchAsync(SearchTicketRequest request);
        /// <summary>
        /// Update an existed ticket
        /// </summary>
        /// <param name="request">UpdateSalesLeadRequest</param>
        /// <returns>UpdateSalesLeadResponse</returns>
        Task<TicketUpdateResponse> UpdateTicketAsync(TicketUpdateRequest request);

        /// <summary>
        /// Create jira issue 
        /// </summary>
        /// <param name="ticketId"></param>
        /// <returns></returns>
        Task CreateJiraIssueFromTicket(long ticketId);

        /// <summary>
        /// Update jira issue 
        /// </summary>
        /// <param name="ticketId"></param>
        /// <returns></returns>
        Task UpdateJiraIssueFromTicket(long ticketId);
    }
}

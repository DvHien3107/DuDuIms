using Enrich.Core.Infrastructure.Repository;
using Enrich.IMS.Dto.Ticket.Queries;
using Enrich.IMS.Dto.Ticket.Reponses;
using Enrich.IMS.Entities;
using Enrich.Infrastructure.Data.Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Interface.Repositories
{
    public interface ITicketRepository : IGenericRepository<Ticket>
    {
        Task<SearchTicketResponse> SearchAsync(SearchTicketRequest request);

        Task UpdateTicketAsync(bool isNew, Ticket ticket, Func<long, Task> extendSave = null);

        Task<TicketStatus> GetTicketStatusAsync(long ticketId);
        Task<IEnumerable<TicketType>> GetTicketTypesAsync(long ticketId);

        Task<IEnumerable<UploadMoreFile>> GetTicketAttachmentsAsync(long ticketId);

        Task<bool> UpdateTicketStatusAsync(long ticketId, TicketStatus ticketStatus, bool isNew = false);

        Task<bool> UpdateTicketTypesAsync(long ticketId, List<TicketType> ticketTypes, bool isNew = false);
    }
}

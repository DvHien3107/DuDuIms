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
    public interface ITicketFeedbackRepository : IGenericRepository<TicketFeedback>
    {
        Task<TicketFeedback> GeyByIdAsync(long id);

        Task<long> AddGetAsync(bool isNew, TicketFeedback ticketFeedback);
    }
}

using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.Infrastructure.Data;
using Enrich.Infrastructure.Data.Dapper;
using System.Threading.Tasks;
using System;

namespace Enrich.IMS.Infrastructure.Data.Implement.Repositories
{
    public partial class TicketFeedbackRepository : DapperGenericRepository<TicketFeedback>, ITicketFeedbackRepository
    {
        public async Task<long> AddGetAsync(bool isNew, TicketFeedback ticketFeedback)
        {
            if (isNew)
            {
                await AddAsync(ticketFeedback);
                return ticketFeedback.Id;
            }
            else
            {
                var affect = await UpdateAsync(ticketFeedback);
                if (affect == 0)
                    throw new Exception("Cannot save property");
            }
            return 0;
        }
    }
}

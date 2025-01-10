using Enrich.Core.Infrastructure.Repository;
using Enrich.IMS.Dto.Ticket;
using Enrich.IMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Interface.Repositories
{
    public interface ITicketStatusRepository : IGenericRepository<TicketStatus>, ILookupDataRepository
    {
        /// <summary>
        /// Get status data by Id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        Task<TicketStatusDto> GetByIdAsync(long Id);
        /// <summary>
        /// Get status project id
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        Task<IEnumerable<TicketStatus>> GetTicketStatusByProjectIdAsync(string projectId);
    }
}

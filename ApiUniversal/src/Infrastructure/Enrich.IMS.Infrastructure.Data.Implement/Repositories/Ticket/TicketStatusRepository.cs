using Dapper;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.Ticket;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.Infrastructure.Data;
using Enrich.Infrastructure.Data.Dapper;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Implement.Repositories
{
    public partial class TicketStatusRepository : DapperGenericRepository<TicketStatus>, ITicketStatusRepository
    {
        public TicketStatusRepository(IConnectionFactory connectionFactory)
            : base(connectionFactory)
        {
        }

        public async Task<TicketStatusDto> GetByIdAsync(long Id)
        {
            var query = $"SELECT * FROM {SqlTables.TicketStatus} {SqlScript.NoLock} WHERE Id = {Id}";

            using (var connection = GetDbConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<TicketStatusDto>(query);
            }
        }

        public async Task<IEnumerable<TicketStatus>> GetTicketStatusByProjectIdAsync(string projectId)
        {
            var query = $"SELECT * FROM {SqlTables.TicketStatus} {SqlScript.NoLock} WHERE ProjectId = '{projectId}'";

            using (var connection = GetDbConnection())
            {
                return await connection.QueryAsync<TicketStatus>(query);
            }
        }
    }
}

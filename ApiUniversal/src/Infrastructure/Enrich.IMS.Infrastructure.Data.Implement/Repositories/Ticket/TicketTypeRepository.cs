using Dapper;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.Infrastructure.Data;
using Enrich.Infrastructure.Data.Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Implement.Repositories
{
    public partial class TicketTypeRepository : DapperGenericRepository<TicketType>, ITicketTypeRepository
    {
        public TicketTypeRepository(IConnectionFactory connectionFactory)
            : base(connectionFactory)
        {
        }
        public async Task<IEnumerable<TicketType>> GetTypesByProjectId(string projectId)
        {
            var query = $"SELECT * FROM {SqlTables.TicketType} {SqlScript.NoLock} WHERE ProjectId = '{projectId}'";

            using (var connection = GetDbConnection())
            {
                return await connection.QueryAsync<TicketType>(query);
            }
        }
    }
}

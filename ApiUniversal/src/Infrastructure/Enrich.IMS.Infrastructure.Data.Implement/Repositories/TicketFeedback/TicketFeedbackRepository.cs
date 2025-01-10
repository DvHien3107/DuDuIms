using Dapper;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.Infrastructure.Data;
using Enrich.Infrastructure.Data.Dapper;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Implement.Repositories
{
    public partial class TicketFeedbackRepository : DapperGenericRepository<TicketFeedback>, ITicketFeedbackRepository
    {
        private const string Alias = SqlTables.TicketFeedback;
        public TicketFeedbackRepository(IConnectionFactory connectionFactory)
            : base(connectionFactory)
        {
        }

        public async Task<TicketFeedback> GeyByIdAsync(long id)
        {
            var query = $"SELECT TOP(1) * FROM {Alias} {SqlScript.NoLock} WHERE Id = {id}";
            using (var connection = GetDbConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<TicketFeedback>(query);
            }
        }
    }
}

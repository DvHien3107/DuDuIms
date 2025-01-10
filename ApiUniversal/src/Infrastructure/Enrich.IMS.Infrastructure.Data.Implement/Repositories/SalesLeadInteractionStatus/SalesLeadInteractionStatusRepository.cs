using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.Infrastructure.Data;
using Enrich.Infrastructure.Data.Dapper;

namespace Enrich.IMS.Infrastructure.Data.Implement.Repositories
{
    public partial class SalesLeadInteractionStatusRepository : DapperGenericRepository<SalesLeadInteractionStatus>, ISalesLeadInteractionStatusRepository
    {
        public SalesLeadInteractionStatusRepository(IConnectionFactory connectionFactory)
            : base(connectionFactory)
        {
        }
    }
}

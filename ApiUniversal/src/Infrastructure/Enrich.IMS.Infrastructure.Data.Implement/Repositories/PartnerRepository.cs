using Dapper;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.Infrastructure.Data;
using Enrich.Infrastructure.Data.Dapper;

namespace Enrich.IMS.Infrastructure.Data.Implement.Repositories
{
    public class PartnerRepository : DapperGenericRepository<Partner>, IPartnerRepository
    {
        public PartnerRepository(IConnectionFactory connectionFactory)
            : base(connectionFactory)
        {
        }

        public Partner GetByCode(string partnerCode)
        {
            var query = $"SELECT TOP 1 * FROM {SqlTables.Partner} WITH (NOLOCK) WHERE Code = @partnerCode";
            var parameters = new DynamicParameters();
            parameters.Add("partnerCode", partnerCode);
            using (var connection = GetDbConnection())
            {
                return connection.QueryFirstOrDefault<Partner>(query, parameters);
            }
        }
    }
}

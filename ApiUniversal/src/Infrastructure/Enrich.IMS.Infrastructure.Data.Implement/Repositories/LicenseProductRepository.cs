using Dapper;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.Infrastructure.Data;
using Enrich.Infrastructure.Data.Dapper;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Implement.Repositories
{
    public class LicenseProductRepository : DapperGenericRepository<LicenseProduct>, ILicenseProductRepository
    {
        public LicenseProductRepository(IConnectionFactory connectionFactory)
            : base(connectionFactory)
        {
        }
        public async Task<LicenseProduct> GetBySubscriptionCodeAsync(string subscriptionCode)
        {
            var query = $"SELECT TOP 1 * FROM {SqlTables.LicenseProduct} WITH (NOLOCK) WHERE Code = @subscriptionCode";
            var parameters = new DynamicParameters();
            parameters.Add("subscriptionCode", subscriptionCode);
            using (var connection = GetDbConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<LicenseProduct>(query, parameters);
            }
        }
    }
}

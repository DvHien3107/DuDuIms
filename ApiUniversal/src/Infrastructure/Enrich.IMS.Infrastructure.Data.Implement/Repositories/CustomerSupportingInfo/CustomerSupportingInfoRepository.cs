using Dapper;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.Infrastructure.Data;
using Enrich.Infrastructure.Data.Dapper;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Implement.Repositories
{
    public class CustomerSupportingInfoRepository : DapperGenericRepository<CustomerSupportingInfo>, ICustomerSupportingInfoRepository
    {
        public CustomerSupportingInfoRepository(IConnectionFactory connectionFactory)
            : base(connectionFactory)
        {
        }

        public async Task<IEnumerable<CustomerSupportingInfoDto>> GetByCustomerIdAsync(long CustomerId)
        {
            var query = SqlScript.CustomerSupportingInfo.GetByCustomerId;
            var parameters = new DynamicParameters();
            parameters.Add("customerId", CustomerId);
            using (var connection = GetDbConnection())
            {
                return await connection.QueryAsync<CustomerSupportingInfoDto>(query, parameters);
            }
        }
    }
}

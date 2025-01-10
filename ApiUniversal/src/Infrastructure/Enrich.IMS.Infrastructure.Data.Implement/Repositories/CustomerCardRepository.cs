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
    public class CustomerCardRepository : DapperGenericRepository<CustomerCard>, ICustomerCardRepository
    {
        public CustomerCardRepository(IConnectionFactory connectionFactory)
            : base(connectionFactory)
        {
        }
        public CustomerCard GetByMxMerchant(long? MxMerchantId)
        {
            var queryPartner = SqlScript.PartnerCard.GetByMxMerchantId;
            var query = SqlScript.CustomerCard.GetByMxMerchantId;
            var parameters = new DynamicParameters();
            parameters.Add(SqlScript.Parameters.MxMerchantId, MxMerchantId);
            using (var connection = GetDbConnection())
            {
                var card = connection.QueryFirstOrDefault<CustomerCard>(queryPartner, parameters);
                if (card != null) return card;
                return connection.QueryFirstOrDefault<CustomerCard>(query, parameters);
            }
        }

        public async Task<CustomerCard> GetByDefaultAsync(string customerCode)
        {
            var query = SqlScript.CustomerCard.GetDefaultByCustomerCode;
            var parameters = new DynamicParameters();
            parameters.Add(SqlScript.Parameters.CustomerCode, customerCode);
            using (var connection = GetDbConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<CustomerCard>(query, parameters);
            }
        }

        public CustomerCard GetByDefault(string customerCode)
        {
            var query = SqlScript.CustomerCard.GetDefaultByCustomerCode;
            var parameters = new DynamicParameters();
            parameters.Add(SqlScript.Parameters.CustomerCode, customerCode);
            using (var connection = GetDbConnection())
            {
                return connection.QueryFirstOrDefault<CustomerCard>(query, parameters);
            }
        }

        public CustomerCard GetForRecurring(long? MxMerchantId, string customerCode)
        {
            var card = GetByMxMerchant(MxMerchantId);
            if (card != null) return card;
            return GetByDefault(customerCode);
        }

        public async Task<IEnumerable<CustomerCardDto>> GetByCustomerCodeAsync(string customerCode)
        {
            var query = SqlScript.CustomerCard.GetByCustomerCode;
            var parameters = new DynamicParameters();
            parameters.Add(SqlScript.Parameters.CustomerCode, customerCode);
            using (var connection = GetDbConnection())
            {
                return await connection.QueryAsync<CustomerCardDto>(query, parameters);
            }
        }
    }
}
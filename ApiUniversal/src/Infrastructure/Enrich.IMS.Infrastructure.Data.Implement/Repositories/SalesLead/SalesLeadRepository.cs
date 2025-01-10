using Dapper;
using Enrich.Common.Enums;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.Infrastructure.Data;
using Enrich.Infrastructure.Data.Dapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace Enrich.IMS.Infrastructure.Data.Implement.Repositories
{
    public partial class SalesLeadRepository : DapperGenericRepository<SalesLead>, ISalesLeadRepository
    {
        private const string AliasSalesLead = SqlTables.SalesLead;

        public SalesLeadRepository(IConnectionFactory connectionFactory)
            : base(connectionFactory)
        {
        }

        /// <summary>
        /// Validation email is exist in data
        /// </summary>
        /// <param name="email"></param>
        /// <returns>
        /// Return true when it exists.
        /// Return false when it not exists.
        /// </returns>
        public bool IsExistEmail(string email)
        {
            var query = SqlScript.SalesLead.CountEmail;
            var parameters = new DynamicParameters();
            parameters.Add("email", email);
            using (var connection = GetDbConnection())
            {
                var count = connection.QueryFirstOrDefault<int>(query, parameters);
                return count > 0;
            }
        }
        public SalesLead GetByCustomerCode(string customercode)
        {
            var query = SqlScript.SalesLead.GetByCustomerCode;
            var parameters = new DynamicParameters();
            parameters.Add("customercode", customercode);
            using (var connection = GetDbConnection())
            {
                return connection.QueryFirstOrDefault<SalesLead>(query, parameters);
            }
        }
        public async Task<SalesLead> GetByCustomerCodeAsync(string customercode)
        {
            var query = SqlScript.SalesLead.GetByCustomerCode;
            var parameters = new DynamicParameters();
            parameters.Add("customercode", customercode);
            using (var connection = GetDbConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<SalesLead>(query, parameters);
            }
        }

        /// <summary>
        /// Generate new customer code
        /// </summary>
        /// <returns></returns>
        public async Task<string> GenerateCustomerCode()
        {
            var query = SqlScript.SalesLead.GenerateCode;
            using (var connection = GetDbConnection())
            {
                var maximunCode = await connection.QueryFirstOrDefaultAsync<string>(query);
                return $"{SalesLeadEnum.SpecialCode.Customer}{(int.Parse((maximunCode).Substring(1)) + 1).ToString().PadLeft(5, '0')}";
            }
        }

        public async Task<Customer> GetSalesLeadCustomerAsync(string salesLeadId)
        {
            var query = $"SELECT c.* FROM C_Customer c INNER JOIN C_SalesLead s ON c.CustomerCode = s.CustomerCode AND s.Id = @salesLeadId";

            using (var connection = GetDbConnection())
            {
                return await connection.QuerySingleOrDefaultAsync<Customer>(query, new { salesLeadId = salesLeadId });
            }
        }


        /// <summary>
        /// Delete multiple salesLead
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task<int> DeleteSalesLeadsAsync(IEnumerable<string> ids)
        {
            var query = $"delete C_SalesLead  where Id in @salesLeadId";

            using (var connection = GetDbConnection())
            {
                return await connection.ExecuteAsync(query, new { salesLeadId = ids });
            }
        }
    }
}
using Dapper;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.Customer;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.Infrastructure.Data;
using Enrich.Infrastructure.Data.Dapper;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Implement.Repositories
{
    public partial class CustomerRepository : DapperGenericRepository<Customer>, ICustomerRepository
    {
        private const string Alias = SqlTables.Customer;
        private readonly ISystemConfigurationRepository _systemConfigurationRepository;

        public CustomerRepository(IConnectionFactory connectionFactory,
            ISystemConfigurationRepository systemConfigurationRepository
            ) : base(connectionFactory)
        {
            _systemConfigurationRepository = systemConfigurationRepository;
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
            var query = SqlScript.Customer.CountEmail;
            var parameters = new DynamicParameters();
            parameters.Add("email", email);
            using (var connection = GetDbConnection())
            {
                var count = connection.QueryFirstOrDefault<int>(query, parameters);
                return count > 0;
            }
        }

        public bool IsExistACH(string CustomerCode)
        {
            var query = SqlScript.Customer.IsExistACH;
            var parameters = new DynamicParameters();
            parameters.Add("customerCode", CustomerCode);
            using (var connection = GetDbConnection())
            {
                var count = connection.QueryFirstOrDefault<int>(query, parameters);
                return count > 0;
            }
        }

        /// <summary>
        /// Validation merchant is pending delivery
        /// </summary>
        /// <param name="storeCode"></param>
        /// <returns></returns>
        public bool IsPendingDelivery(string storeCode)
        {
            var query = SqlScript.Customer.CheckMerchantPendingDelivery(storeCode);
            using (var connection = GetDbConnection())
            {
                var count = connection.QueryFirstOrDefault<int>(query);
                return count > 0;
            }
        }

        public async Task<Customer> GetById(int Id)
        {
            var query = $"SELECT TOP 1 * FROM {SqlTables.Customer} WITH (NOLOCK) WHERE Id = @id";
            var parameters = new DynamicParameters();
            parameters.Add("id", Id);
            using (var connection = GetDbConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<Customer>(query, parameters);
            }
        }

        public async Task<Customer> GetByStoreCode(string storeCode)
        {
            var query = $"SELECT TOP 1 * FROM {SqlTables.Customer} WITH (NOLOCK) WHERE StoreCode = @storecode";
            var parameters = new DynamicParameters();
            parameters.Add("storecode", storeCode);
            using (var connection = GetDbConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<Customer>(query, parameters);
            }
        }

        public async Task<Customer> GetByCustomerCodeAsync(string CustomerCode)
        {
            var query = $"SELECT TOP 1 * FROM {SqlTables.Customer} WITH (NOLOCK) WHERE CustomerCode = @customercode";
            var parameters = new DynamicParameters();
            parameters.Add("customercode", CustomerCode);
            using (var connection = GetDbConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<Customer>(query, parameters);
            }
        }

        public Customer GetByCustomerCode(string CustomerCode)
        {
            var query = $"SELECT TOP 1 * FROM {SqlTables.Customer} WITH (NOLOCK) WHERE CustomerCode = @customercode";
            var parameters = new DynamicParameters();
            parameters.Add("customercode", CustomerCode);
            using (var connection = GetDbConnection())
            {
                return connection.QueryFirstOrDefault<Customer>(query, parameters);
            }
        }

        public async Task<CustomerDto> GetByIdAsync(long Id)
        {
            var query = SqlScript.Customer.GetById;
            var parameters = new DynamicParameters();
            parameters.Add(SqlScript.Customer.Parameters.Id, Id);
            using (var connection = GetDbConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<CustomerDto>(query, parameters);
            }
        }
        public async Task<CustomerBaseDto> GetBaseInfomationByIdAsync(long Id)
        {
            var query = SqlScript.Customer.GetBaseInformation;
            var parameters = new DynamicParameters();
            parameters.Add(SqlScript.Customer.Parameters.Id, Id);
            using (var connection = GetDbConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<CustomerBaseDto>(query, parameters);
            }
        }
    }
}
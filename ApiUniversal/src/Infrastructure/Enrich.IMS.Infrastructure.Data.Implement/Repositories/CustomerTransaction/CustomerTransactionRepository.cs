using Dapper;
using Enrich.BusinessEvents.IMS;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.CustomerTransaction;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.Infrastructure.Data;
using Enrich.Infrastructure.Data.Dapper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Implement.Repositories
{
    public partial class CustomerTransactionRepository : DapperGenericRepository<CustomerTransaction>, ICustomerTransactionRepository
    {
        public CustomerTransactionRepository(IConnectionFactory connectionFactory)
            : base(connectionFactory)
        {
        }

        /// <summary>
        /// Get by customer code
        /// </summary>
        /// <param name="fromDate"></param>
        /// <returns></returns>
        public async Task<IEnumerable<CustomerTransactionDto>> GetByCustomerCodeAsync(string customerCode)
        {
            var query = SqlScript.CustomerTransaction.GetByCustomerCode;
            var parameters = new DynamicParameters();
            parameters.Add(SqlScript.Parameters.CustomerCode, customerCode);
            using (var connection = GetDbConnection())
            {
                return await connection.QueryAsync<CustomerTransactionDto>(query, parameters);
            }
        }

        /// <summary>
        /// Check have success transaction of order
        /// </summary>
        /// <param name="ordercode"></param>
        /// <returns></returns>
        public async Task<bool> HasApproveTransactionAsync(string ordercode)
        {
            var query = SqlScript.CustomerTransaction.CountApproveTransaction;
            var parameters = new DynamicParameters();
            parameters.Add(SqlScript.Parameters.OrderCode, ordercode);
            using (var connection = GetDbConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<int>(query, parameters) > 0;
            }
        }

        /// <summary>
        /// Get success transaction og order
        /// </summary>
        /// <param name="ordercode"></param>
        /// <returns></returns>
        public async Task<CustomerTransaction> GetApproveTransactionAsync(string ordercode)
        {
            var query = SqlScript.CustomerTransaction.GetApproveTransaction;
            var parameters = new DynamicParameters();
            parameters.Add(SqlScript.Parameters.OrderCode, ordercode);
            using (var connection = GetDbConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<CustomerTransaction>(query, parameters);
            }
        }

        /// <summary>
        /// Get failed transaction from date
        /// </summary>
        /// <param name="fromDate"></param>
        /// <returns></returns>
        public async Task<IEnumerable<CustomerTransactionFailed>> GetFailedTransactionFromDateAsync(DateTime fromDate)
        {
            var query = SqlScript.CustomerTransaction.GetFailedTransactionFromDate;
            var parameters = new DynamicParameters();
            parameters.Add(SqlScript.Parameters.FromDate, fromDate);
            using (var connection = GetDbConnection())
            {
                return await connection.QueryAsync<CustomerTransactionFailed>(query, parameters);
            }
        }

        /// <summary>
        /// Get meta data by transaction Id
        /// </summary>
        /// <param name="fromDate"></param>
        /// <returns></returns>
        public async Task<OrderPaymentFailedEvent> GetMetaDataAsync(string transactionId)
        {
            var query = SqlScript.CustomerTransaction.GetMetaDataPaymentFailed;
            var parameters = new DynamicParameters();
            parameters.Add(SqlScript.Parameters.TransactionId, transactionId);
            using (var connection = GetDbConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<OrderPaymentFailedEvent>(query, parameters);
            }
        }
    }
}

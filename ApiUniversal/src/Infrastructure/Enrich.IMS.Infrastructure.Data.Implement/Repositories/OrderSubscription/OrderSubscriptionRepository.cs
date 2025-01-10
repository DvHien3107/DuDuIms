using Dapper;
using Enrich.Common.Enums;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.Infrastructure.Data;
using Enrich.Infrastructure.Data.Dapper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Implement.Repositories
{
    public partial class OrderSubscriptionRepository : DapperGenericRepository<OrderSubscription>, IOrderSubscriptionRepository
    {
        public OrderSubscriptionRepository(IConnectionFactory connectionFactory)
            : base(connectionFactory)
        {
        }

        public async Task DoProc(string query)
        {
            using (var connection = GetDbConnection())
            {
                await connection.ExecuteAsync(query);
            }
        }

        public OrderSubscription GetByOrderCodeNSubscriptionCode(string orderCode, string subscriptionCode)
        {
            var query = $"SELECT TOP 1 * FROM {SqlTables.OrderSubscription} WITH (NOLOCK) WHERE [OrderCode] = @orderCode AND [Product_Code] = @subscriptionCode AND [SubscriptionType] != 'setupfee' AND [SubscriptionType] != 'interactionfee'";
            var parameters = new DynamicParameters();
            parameters.Add("orderCode", orderCode);
            parameters.Add("subscriptionCode", subscriptionCode);
            using (var connection = GetDbConnection())
            {
                return connection.QueryFirstOrDefault<OrderSubscription>(query, parameters);
            }
        }
        public async Task<OrderSubscription> GetByOrderCodeNSubscriptionCodeAsync(string orderCode, string subscriptionCode)
        {
            var query = $"SELECT TOP 1 * FROM {SqlTables.OrderSubscription} WITH (NOLOCK) WHERE [OrderCode] = @orderCode AND [Product_Code] = @subscriptionCode";
            var parameters = new DynamicParameters();
            parameters.Add("orderCode", orderCode);
            parameters.Add("subscriptionCode", subscriptionCode);
            using (var connection = GetDbConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<OrderSubscription>(query, parameters);
            }
        }
        public IEnumerable<OrderSubscription> GetByOrderCode(string orderCode)
        {
            var query = $"SELECT TOP 1 * FROM {SqlTables.OrderSubscription} WITH (NOLOCK) WHERE [OrderCode] = @orderCode";
            var parameters = new DynamicParameters();
            parameters.Add("orderCode", orderCode);
            using (var connection = GetDbConnection())
            {
                return connection.Query<OrderSubscription>(query, parameters);
            }
        }
        public async Task<IEnumerable<OrderSubscription>> GetByOrderCodeAsync(string orderCode)
        {
            var query = $"SELECT TOP 1 * FROM {SqlTables.OrderSubscription} WITH (NOLOCK) WHERE [OrderCode] = @orderCode";
            var parameters = new DynamicParameters();
            parameters.Add("orderCode", orderCode);
            using (var connection = GetDbConnection())
            {
                return await connection.QueryAsync<OrderSubscription>(query, parameters);
            }
        }

        public DateTime CalculatorEndDate(OrderSubscription orderSubscription)
        {
            if(orderSubscription == null) return DateTime.UtcNow;
            var endDate = orderSubscription.EndDate ?? DateTime.UtcNow;
            if (orderSubscription.PeriodRecurring == SubscriptionEnum.RecurringInterval.Yearly.ToString())
                orderSubscription.EndDate = endDate.AddYears(orderSubscription.Quantity ?? 1);
            else if (orderSubscription.PeriodRecurring == SubscriptionEnum.RecurringInterval.Weekly.ToString())
                orderSubscription.EndDate = endDate.AddDays(orderSubscription.Quantity ?? 1 * 7);
            else orderSubscription.EndDate = endDate.AddMonths(orderSubscription.Quantity ?? 1);
            return orderSubscription.EndDate.Value;
        }
    }
}

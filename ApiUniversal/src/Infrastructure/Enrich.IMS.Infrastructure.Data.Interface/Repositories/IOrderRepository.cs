using Enrich.Core.Infrastructure.Repository;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Interface.Repositories
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        public Order GetByOrderCode(string orderCode);
        public Task<Order> GetByOrderCodeAsync(string orderCode);
        public int GetNumberOrderOnDate();
        public Task<int> GetNumberOrderOnDateAsync();
        public Task<IEnumerable<OrderSubscription>> GetOrderSubscriptions(string OrderCode);
        public Task<bool> CreateAsync(OrderDto order, List<OrderSubscriptionDto> orderSubscriptions);
        public Task<bool> ChangeStatusAsync(string orderId, string status);

        /// <summary>
        /// Get list order payment later from fromDate
        /// </summary>
        /// <param name="dateEffect"></param>
        /// <returns>List order</returns>
        public Task<IEnumerable<OrderDto>> GetPaymentLaterByDateAsync(DateTime fromDate);
    }
}
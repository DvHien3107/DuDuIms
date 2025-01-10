using Enrich.Core.Infrastructure.Repository;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.OrderSubscription;
using Enrich.IMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Interface.Repositories
{
    public interface IOrderSubscriptionRepository : IGenericRepository<OrderSubscription>
    {
        Task<OrderSubscriptionSearchResponse> SearchAsync(OrderSubscriptionSearchRequest request);
        Task<OrderSubscriptionSearchResponse> SearchAsyncByProc(OrderSubscriptionSearchRequest request);

        public OrderSubscription GetByOrderCodeNSubscriptionCode(string orderCode, string subscriptionCode);
        public Task<OrderSubscription> GetByOrderCodeNSubscriptionCodeAsync(string orderCode, string subscriptionCode);
        public Task<IEnumerable<OrderSubscription>> GetByOrderCodeAsync(string orderCode);
        public IEnumerable<OrderSubscription> GetByOrderCode(string orderCode);
        public DateTime CalculatorEndDate(OrderSubscription orderSubscription);
        Task DoProc(string query);
        Task<SubscriptionPackageSearchResponse> SearchSubscriptionPackageAsync(SubscriptionPackageSearchRequest request);
    }
}

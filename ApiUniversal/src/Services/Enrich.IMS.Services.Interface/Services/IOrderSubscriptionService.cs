using Enrich.Core.Infrastructure.Services;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.OrderSubscription;
using Enrich.IMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Interface.Services
{
    public interface IOrderSubscriptionService : IGenericService<OrderSubscription, OrderSubscriptionDto>
    {
        public Task<OrderSubscription> GetByOrderCodeNSubscriptionCodeAsync(string orderCode, string subscriptionCode);
        public OrderSubscription GetByOrderCodeNSubscriptionCode(string orderCode, string subscriptionCode);
        Task DoProc(string query);
        Task<OrderSubscriptionSearchResponse> SearchAsync(OrderSubscriptionSearchRequest request);
        Task<OrderSubscriptionSearchResponse> SearchAsyncByProc(OrderSubscriptionSearchRequest request);

        Task<SubscriptionPackageSearchResponse> SearchSubscriptionPackageAsync(string package, SubscriptionPackageSearchRequest request);
    }
}

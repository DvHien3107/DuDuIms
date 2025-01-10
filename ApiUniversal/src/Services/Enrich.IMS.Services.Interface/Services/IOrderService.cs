using Enrich.Core.Infrastructure.Services;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Interface.Services
{
    public interface IOrderService : IGenericService<Order, OrderDto> 
    {
        public Task<Order> GetByOrderCodeAsync(string orderCode);
        public Task<Order> CreateAsync(Order order, List<OrderSubscription> orderSubscriptions);
        public Task<bool> ChangeStatusAsync(string orderCode, string status);
    }
}

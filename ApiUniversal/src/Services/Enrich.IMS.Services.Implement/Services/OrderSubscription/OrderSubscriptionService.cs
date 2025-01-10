using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.IMS.Services.Interface.Mappers;
using Enrich.IMS.Services.Interface.Services;
using Enrich.Services.Implement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Services
{
    public partial class OrderSubscriptionService : GenericService<OrderSubscription, OrderSubscriptionDto>, IOrderSubscriptionService
    {
        private IOrderSubscriptionMapper _mapper => _mapperGeneric as IOrderSubscriptionMapper;
        private readonly IOrderSubscriptionRepository _repository;


        public OrderSubscriptionService(IOrderSubscriptionRepository repository, IOrderSubscriptionMapper mapper)
            : base(repository, mapper)
        {
            _repository = repository;
        }
        public OrderSubscription GetByOrderCodeNSubscriptionCode(string orderCode, string subscriptionCode)
        {
            return _repository.GetByOrderCodeNSubscriptionCode(orderCode, subscriptionCode);
        }
        public async Task<OrderSubscription> GetByOrderCodeNSubscriptionCodeAsync(string orderCode, string subscriptionCode)
        {
            return await _repository.GetByOrderCodeNSubscriptionCodeAsync(orderCode, subscriptionCode);
        }

        public async Task DoProc(string query) {
            await _repository.DoProc(query);
        }
    }
}

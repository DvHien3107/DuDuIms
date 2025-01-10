using Pos.Application.Repository.IMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Application.Services.Scoped
{
    public interface IStoreService
    {
        Task UpdateRecurringCard(string cardAccountId, string SubscriptionId, string StoreServiceId, bool AutoRenew);
        Task UpdateStoreService(string CustomerCode);
    }
    public class StoreService : IStoreService
    {
        IStoreServiceRepository _storeServiceRepository;
        public StoreService(IStoreServiceRepository storeServiceRepository) {
            _storeServiceRepository = storeServiceRepository;
        }

        public async Task UpdateRecurringCard(string cardAccountId, string SubscriptionId, string StoreServiceId, bool AutoRenew)
        {
            await _storeServiceRepository.UpdateRecurringCard(cardAccountId, SubscriptionId, StoreServiceId, AutoRenew);
        }

        public async Task UpdateStoreService(string CustomerCode)
        {
            await _storeServiceRepository.UpdateStoreService(CustomerCode);
        }
    }
}

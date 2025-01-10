using Enrich.Core.Infrastructure.Repository;
using Enrich.Dto.Base.POSApi;
using Enrich.IMS.Dto.Subscription;
using Enrich.IMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Interface.Repositories
{
    public interface IStoreServiceRepository : IGenericRepository<StoreService>
    {
        public IEnumerable<StoreService> GetByOrderCode(string orderCode);
        public StoreService GetByOrderCodeNSubscriptionCode(string orderCode, string subscriptionCode);
        public Task<StoreService> GetByLastOrderCodeAsync(string lastOrderCode, string subscriptionCode);
        public Task<StoreService> GetLicenseActivatedAsync(string customerCode, string subscriptionCode);
        public Task<StoreService> GetLicenseActivatedAsync(string customerCode);

        Task<IEnumerable<BaseService>> GetBaseServiceByStoreServiceIdAsync(string storeServiceId);

        public Task<IEnumerable<BaseService>> GetBaseServiceAsync(string customerCode, string storeServiceId, bool addon = false);
        public Task<IEnumerable<BaseService>> GetBaseServiceAsync(string customerCode);
        public Task<IEnumerable<FeatureBase>> GetFeatureAsync(string customerCode);
        public Task<StoreService> UpdateGetRecurringAsync(string orderCode, string orderCodeRecurring, string subscriptionCode);

        /// <summary>
        /// Get license status for population search merchant
        /// </summary>
        /// <param name="listStoreCode"></param>
        /// <returns></returns>
        public Task<IEnumerable<LicenseStatusDto>> GetLicenseStatusAsync(IEnumerable<string> listStoreCode);
    }
}

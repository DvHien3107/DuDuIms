using Dapper;
using Pos.Model.Model.Table.IMS;
using Promotion.Application.Extensions;
using Promotion.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Application.Repository.IMS
{
    public interface IStoreServiceRepository : IEntityService<Store_Services>
    {
        Task UpdateStoreService(string CustomerCode);
        Task UpdateRecurringCard(string cardAccountId, string SubscriptionId, string StoreServiceId, bool AutoRenew);
    }
    public class StoreServiceRepository : IMSEntityService<Store_Services>, IStoreServiceRepository
    {
        public async Task UpdateRecurringCard(string cardAccountId, string SubscriptionId, string StoreServiceId, bool AutoRenew)
        {
            await _connection.ExecuteAsync("update Store_Services set MxMerchant_cardAccountId = @cardAccountId, MxMerchant_SubscriptionId = @SubscriptionId, AutoRenew = @AutoRenew where Id = @StoreServiceId", new { cardAccountId, SubscriptionId, StoreServiceId, AutoRenew });
        }
        public async Task UpdateStoreService(string CustomerCode)
        {
            await _connection.ExecuteAsync("EXEC P_UpdateStoreService @CustomerCode", new { CustomerCode });
        }
    }
}

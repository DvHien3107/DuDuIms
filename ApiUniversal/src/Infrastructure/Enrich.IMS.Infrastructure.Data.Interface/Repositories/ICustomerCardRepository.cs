using Enrich.Core.Infrastructure.Repository;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Interface.Repositories
{
    public interface ICustomerCardRepository : IGenericRepository<CustomerCard>
    {
        /// <summary>
        /// Get card by MxMerchant Id
        /// </summary>
        /// <param name="MxMerchantId"></param>
        /// <returns>Customer Card</returns>
        CustomerCard GetByMxMerchant(long? MxMerchantId);

        /// <summary>
        /// Get default card of customer code async
        /// </summary>
        /// <param name="customerCode"></param>
        /// <returns></returns>
        Task<CustomerCard> GetByDefaultAsync(string customerCode);

        /// <summary>
        /// Get default card of customer code
        /// </summary>
        /// <param name="customerCode"></param>
        /// <returns>Customer Card</returns>
        CustomerCard GetByDefault (string customerCode);

        /// <summary>
        /// Get card for recurring
        /// </summary>
        /// <param name="MxMerchantId"></param>
        /// <param name="customerCode"></param>
        /// <returns>Customer Card</returns>
        CustomerCard GetForRecurring(long? MxMerchantId, string customerCode);

        /// <summary>
        /// Get list card by customer code
        /// </summary>
        /// <param name="customerCode"></param>
        /// <returns></returns>
        Task<IEnumerable<CustomerCardDto>> GetByCustomerCodeAsync(string customerCode);
    }
}
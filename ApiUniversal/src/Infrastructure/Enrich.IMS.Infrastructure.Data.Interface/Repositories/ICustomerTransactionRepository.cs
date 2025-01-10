using Enrich.BusinessEvents.IMS;
using Enrich.Core.Infrastructure.Repository;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.CustomerTransaction;
using Enrich.IMS.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Interface.Repositories
{
    public interface ICustomerTransactionRepository : IGenericRepository<CustomerTransaction>
    {
        /// <summary>
        /// Get list transaction by customer code
        /// </summary>
        /// <param name="customerCode"></param>
        /// <returns></returns>
        Task<IEnumerable<CustomerTransactionDto>> GetByCustomerCodeAsync(string customerCode);


        Task<bool> HasApproveTransactionAsync(string ordercode);
        Task<CustomerTransaction> GetApproveTransactionAsync(string ordercode);
        Task<IEnumerable<CustomerTransactionFailed>> GetFailedTransactionFromDateAsync(DateTime fromDate);
        Task<OrderPaymentFailedEvent> GetMetaDataAsync(string transactionId);

        Task<CustomerTransactionSearchResponse> SearchAsync(CustomerTransactionSearchRequest request);
    }
}

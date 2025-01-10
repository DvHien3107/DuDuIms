using Enrich.Core.Infrastructure.Services;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.CustomerTransaction;
using Enrich.IMS.Entities;
using Enrich.Payment.MxMerchant.Models;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Interface.Services
{
    public interface ICustomerTransactionService : IGenericService<CustomerTransaction, CustomerTransactionDto>
    {
        public void Save_MxMerchantTokens(OauthInfo auth);
        public Task<CustomerTransaction> CreateWithFree(Order order);
        public Task<CustomerTransaction> CreateWithCreditCard(Order order, CustomerCard card);
        public Task<CustomerTransaction> CreateWithACH(Order order);
        public Task ActionPaymentFailed(Order order, Customer customer, CustomerTransaction customerTransaction);

        Task<CustomerTransactionSearchResponse> SearchAsync(CustomerTransactionSearchRequest request);
    }
}
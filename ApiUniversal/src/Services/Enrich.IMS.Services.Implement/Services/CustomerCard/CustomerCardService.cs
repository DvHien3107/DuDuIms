using Enrich.Dto;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.IMS.Services.Interface.Mappers;
using Enrich.IMS.Services.Interface.Services;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Services
{
    public partial class CustomerCardService : EnrichBaseService<CustomerCard, CustomerCardDto>, ICustomerCardService
    {
        #region field
        private readonly EnrichContext _context;
        private ICustomerCardMapper _mapper => _mapperGeneric as ICustomerCardMapper;
        private readonly ICustomerCardRepository _repository;
        private readonly ICustomerRepository _repositoryCustomer;
        #endregion

        #region contructor
        public CustomerCardService(
            ICustomerCardRepository repository,
            IMemberMapper mapper,
            ICustomerRepository repositoryCustomer)
            : base(repository, mapper)
        {
            _repository = repository;
            _repositoryCustomer = repositoryCustomer;
        }
        #endregion

        public async Task<string> GetRecurringCardAsync(string customerCode)
        {
            var card = await _repository.GetByDefaultAsync(customerCode);
            if (card != null)
                return $"{card.CardNumber}";
            var customer = await _repositoryCustomer.GetByCustomerCodeAsync(customerCode) ?? new Customer { };
            if (customer != null && !string.IsNullOrEmpty(customer.DepositBankName) && !string.IsNullOrEmpty(customer.DepositAccountNumber))
                return $"{customer.DepositBankName} - {customer.DepositAccountNumber}";
            return "N/A";
        }
    }
}
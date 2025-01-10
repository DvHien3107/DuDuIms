using Enrich.Core.Container;
using Enrich.Dto;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.IMS.Services.Interface.Mappers;
using Enrich.IMS.Services.Interface.Services;
using Enrich.Services.Implement;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Services
{
    public partial class CustomerService : GenericService<Customer, CustomerDto>, ICustomerService
    {
        private ICustomerMapper _mapper => _mapperGeneric as ICustomerMapper;
        private readonly ICustomerRepository _repository;
        private readonly EnrichContext _context;
        private readonly IEnrichContainer _container;
        private readonly IStoreServiceRepository _storeServiceRepository;
        private readonly INewCustomerGoalRepository _newCustomerGoalRepository;

        public CustomerService(
            ICustomerRepository repository,
            ICustomerMapper mapper,
            EnrichContext context,
            IEnrichContainer container, 
            IStoreServiceRepository storeServiceRepository,
            INewCustomerGoalRepository newCustomerGoalRepository)
            : base(repository, mapper)
        {
            _repository = repository;
            _context = context;
            _container = container;
            _storeServiceRepository = storeServiceRepository;
            _newCustomerGoalRepository = newCustomerGoalRepository;
        }

        public async Task<Customer> GetById(int id)
        {
            return await _repository.GetById(id);
        }
        public async Task<Customer> GetByStoreCode(string storeCode)
        {
            return await _repository.GetByStoreCode(storeCode);
        }
        public async Task<Customer> GetByCustomerCodeAsync(string customerCode)
        {
            return await _repository.GetByCustomerCodeAsync(customerCode);
        }
    }
}

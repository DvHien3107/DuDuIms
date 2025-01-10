using Enrich.Dto;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface;
using Enrich.IMS.Services.Interface.Mappers;
using Enrich.IMS.Services.Interface.Services;
using Enrich.Services.Implement;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Services
{
    public class BusinessEventService : GenericService<BusinessEvent, BusinessEventDto>, IBusinessEventService
    {
        private readonly IBusinessEventRepository _repository;
        private readonly EnrichContext _context;
        public BusinessEventService(IBusinessEventRepository repository, IBusinessEventMapper mapper, EnrichContext context)
            : base(repository, mapper)
        {
            _repository = repository;
            _context = context;
        }
    }
}
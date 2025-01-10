using Enrich.Dto;
using Enrich.IMS.Dto.Enums;
using Enrich.IMS.Dto.Lookup;
using Enrich.IMS.Dto.Ticket;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.IMS.Services.Interface.Mappers;
using Enrich.IMS.Services.Interface.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Services
{
    public partial class TicketTypeService : EnrichBaseService<TicketType, TicketTypeDto>, ITicketTypeService
    {
        private readonly ITicketTypeRepository _repository;
        private ITicketTypeMapper _mapper => _mapperGeneric as ITicketTypeMapper;
        public TicketTypeService(
            ITicketTypeMapper mapper,
            ITicketTypeRepository repository
            ) : base(repository, mapper)
        {
            _repository = repository;
        }

        /// <summary>
        /// Optimize request for get lookup data
        /// </summary>
        /// <param name="request"></param>
        public void OptimizeConditionRequest(LookupDataRequest request, LookupDataType type)
        {
            request.FieldNames.Add(new SqlMapDto("Id", "Value"));
            request.FieldNames.Add(new SqlMapDto("TypeName", "Name"));
        }

        public async Task<IEnumerable<TicketTypeDto>> GetTicketTypesByProjectIdAsync(string projectId)
        {
            var types = await _repository.GetTypesByProjectId(projectId);
            return _mapper.Map<IEnumerable<TicketTypeDto>>(types);
        }

    }
}

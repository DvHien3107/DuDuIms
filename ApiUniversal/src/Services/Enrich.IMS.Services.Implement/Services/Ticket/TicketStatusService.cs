using Enrich.Dto;
using Enrich.IMS.Dto.Enums;
using Enrich.IMS.Dto.Lookup;
using Enrich.IMS.Dto.Ticket;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.IMS.Services.Interface.Mappers;
using Enrich.IMS.Services.Interface.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Services
{
    public partial class TicketStatusService : EnrichBaseService<TicketStatus, TicketStatusDto>, ITicketStatusService
    {
        private readonly ITicketStatusRepository _repository;
        private ITicketStatusMapper _mapper => _mapperGeneric as ITicketStatusMapper;
        public TicketStatusService(
            ITicketStatusMapper mapper,
            ITicketStatusRepository repository
            ) : base(repository, mapper)
        {
            _repository = repository;
        }

        public void OptimizeConditionRequest(LookupDataRequest request, LookupDataType type)
        {
            request.FieldNames.Add(new SqlMapDto("Id", "Value"));
            request.FieldNames.Add(new SqlMapDto("Name", "Name"));
        }
        public async Task<IEnumerable<TicketStatusDto>> GetTicketStatusByProjectAsync(string projectId)
        {
            var statuses = await _repository.GetTicketStatusByProjectIdAsync(projectId);
            return _mapper.Map<IEnumerable<TicketStatusDto>>(statuses);
        }
    }
}

using Enrich.Core.Container;
using Enrich.Dto;
using Enrich.Dto.Base;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.Enums;
using Enrich.IMS.Dto.Lookup;
using Enrich.IMS.Dto.SalesLeadComment;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Implement.Repositories;
using Enrich.IMS.Infrastructure.Data.Interface.Builders;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.IMS.Services.Interface.Mappers;
using Enrich.IMS.Services.Interface.Services;
using Enrich.Infrastructure.Data.Dapper.Library;
using System.Linq;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Services
{
    public partial class TicketFeedbackService : EnrichBaseService<TicketFeedback, TicketFeedbackDto>, ITicketFeedbackService
    {
        private ITicketFeedbackMapper _mapper => _mapperGeneric as ITicketFeedbackMapper;
        private readonly EnrichContext _context;
        private readonly IEnrichContainer _container;
        private readonly ITicketFeedbackBuilder _builder;
        private readonly ITicketFeedbackRepository _repository;
        private readonly ITicketRepository _ticketRepository;

        public TicketFeedbackService(EnrichContext context,
            ITicketFeedbackMapper mapper,
            ITicketFeedbackRepository repository,
            IEnrichContainer container,
            ITicketRepository ticketRepository,
            ITicketFeedbackBuilder builder) : base(repository, mapper)
        {
            _context = context;
            _container = container;
            _repository = repository;
            _ticketRepository = ticketRepository;
            _builder = builder;
        }

        public async Task<TicketFeedbackDto> GetByIdAsync(long id)
        {
            var entity = await _repository.GeyByIdAsync(id);
            var dto = _mapper.Map<TicketFeedbackDto>(entity);
            return dto;
        }
    }
}
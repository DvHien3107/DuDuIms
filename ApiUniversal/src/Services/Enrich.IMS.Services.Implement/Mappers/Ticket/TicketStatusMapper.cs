using Enrich.IMS.Dto.Ticket;
using Enrich.IMS.Entities;
using Enrich.IMS.Services.Interface.Mappers;
using Enrich.Services.Implement;

namespace Enrich.IMS.Services.Implement.Mappers
{
    public class TicketStatusMapper : BaseMapperAutoMapper<TicketStatus, TicketStatusDto>, ITicketStatusMapper
    {
        public TicketStatusMapper() : base()
        {
        }
    }
}

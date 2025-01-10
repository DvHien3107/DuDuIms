using Enrich.IMS.Dto.Ticket;
using Enrich.IMS.Entities;
using Enrich.IMS.Services.Interface.Mappers;
using Enrich.Services.Implement;

namespace Enrich.IMS.Services.Implement.Mappers
{
    public class TicketTypeMapper : BaseMapperAutoMapper<TicketType, TicketTypeDto>, ITicketTypeMapper
    {
        public TicketTypeMapper() : base()
        {
        }
    }
}

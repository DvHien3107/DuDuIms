using Dapper.FluentMap.Dommel.Mapping;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    public class TicketEntityMapper : DommelEntityMap<Ticket>
    {
        public TicketEntityMapper()
        {
            ToTable(SqlTables.SupportTicket);
            Map(p => p.Id).IsKey().IsIdentity();
        }
    }
}

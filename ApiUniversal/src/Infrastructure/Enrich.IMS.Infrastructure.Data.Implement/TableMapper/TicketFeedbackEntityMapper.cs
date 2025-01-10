using Dapper.FluentMap.Dommel.Mapping;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    public class TicketFeedbackEntityMapper : DommelEntityMap<TicketFeedback>
    {
        public TicketFeedbackEntityMapper()
        {
            ToTable(SqlTables.TicketFeedback);
            Map(p => p.Id).IsKey().IsIdentity();
        }
    }
}

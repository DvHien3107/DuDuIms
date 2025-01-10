using Dapper.FluentMap.Dommel.Mapping;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    public class SalesLeadInteractionStatusEntityMapper : DommelEntityMap<SalesLeadInteractionStatus>
    {
        public SalesLeadInteractionStatusEntityMapper()
        {
            ToTable(SqlTables.SalesLeadInteractionStatus);
            Map(p => p.Id).IsKey();
        }
    }
}

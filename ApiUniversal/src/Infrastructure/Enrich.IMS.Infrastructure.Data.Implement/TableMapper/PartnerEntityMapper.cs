using Dapper.FluentMap.Dommel.Mapping;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    public class PartnerEntityMapper : DommelEntityMap<Partner>
    {
        public PartnerEntityMapper()
        {
            ToTable(SqlTables.Partner);
            Map(p => p.Id).IsKey();
        }
    }
}

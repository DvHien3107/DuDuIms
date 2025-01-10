using Dapper.FluentMap.Dommel.Mapping;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    public class BusinessEventEntityMapper : DommelEntityMap<BusinessEvent>
    {
        public BusinessEventEntityMapper()
        {
            ToTable(SqlTables.BusinessEvent);
            Map(p => p.Id).IsKey();
        }
    }
}
using Dapper.FluentMap.Dommel.Mapping;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    public class CustomerCardEntityMapper : DommelEntityMap<CustomerCard>
    {
        public CustomerCardEntityMapper()
        {
            ToTable(SqlTables.CustomerCard);
            Map(p => p.Id).IsKey();
        }
    }
}

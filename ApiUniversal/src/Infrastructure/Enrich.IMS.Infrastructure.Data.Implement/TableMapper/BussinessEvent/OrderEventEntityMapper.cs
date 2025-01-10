using Dapper.FluentMap.Dommel.Mapping;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    public class OrderEventEntityMapper : DommelEntityMap<OrderEvent>
    {
        public OrderEventEntityMapper()
        {
            ToTable(SqlTables.OrderEvent);
            Map(p => p.Id).IsKey();
        }
    }
}
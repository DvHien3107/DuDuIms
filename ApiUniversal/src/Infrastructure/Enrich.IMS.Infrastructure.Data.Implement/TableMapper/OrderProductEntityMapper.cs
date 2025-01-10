using Dapper.FluentMap.Dommel.Mapping;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    public class OrderProductEntityMapper : DommelEntityMap<OrderProducts>
    {
        public OrderProductEntityMapper()
        {
            ToTable(SqlTables.OrderProducts);
            Map(p => p.Id).IsKey();
        }
    }
}

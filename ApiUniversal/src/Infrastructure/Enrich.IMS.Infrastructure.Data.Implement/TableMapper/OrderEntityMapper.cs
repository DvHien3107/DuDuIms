using Dapper.FluentMap.Dommel.Mapping;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    public class OrderEntityMapper : DommelEntityMap<Order>
    {
        public OrderEntityMapper()
        {
            ToTable(SqlTables.Orders);
            Map(p => p.Id).IsKey();
            Map(p => p.NoteDelivery).ToColumn(SqlColumns.Orders.NoteDelivery);
            Map(p => p.NotePackaging).ToColumn(SqlColumns.Orders.NotePackaging);
            Map(p => p.ServiceAmount).ToColumn(SqlColumns.Orders.ServiceAmount);
            Map(p => p.TotalHardwareAmount).ToColumn(SqlColumns.Orders.TotalHardwareAmount);
        }
    }
}

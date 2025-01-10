using Dapper.FluentMap.Dommel.Mapping;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    public class OrderSubscriptionEntityMapper : DommelEntityMap<OrderSubscription>
    {
        public OrderSubscriptionEntityMapper()
        {
            ToTable(SqlTables.OrderSubscription);
            Map(p => p.Id).IsKey();
            Map(p => p.ProductCode).ToColumn(SqlColumns.OrderSubcription.ProductCode);
            Map(p => p.ProductCodePOSSystem).ToColumn(SqlColumns.OrderSubcription.ProductCodePOSSystem);
            Map(p => p.PromotionApplyMonths).ToColumn(SqlColumns.OrderSubcription.PromotionApplyMonths);
        }
    }
}

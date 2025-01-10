using Dapper.FluentMap.Dommel.Mapping;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    public class CustomerEntityMapper : DommelEntityMap<Customer>
    {
        public CustomerEntityMapper()
        {
            ToTable(SqlTables.Customer);
            Map(p => p.MxMerchantId).ToColumn(SqlColumns.Customer.MxMerchantId);
            Map(p => p.SalonTimeZoneNumber).ToColumn(SqlColumns.Customer.SalonTimeZoneNumber);
            Map(p => p.MoreInfo).ToColumn(SqlColumns.Customer.MoreInfo);
            Map(p => p.Id).IsKey();            
        }
    }
}

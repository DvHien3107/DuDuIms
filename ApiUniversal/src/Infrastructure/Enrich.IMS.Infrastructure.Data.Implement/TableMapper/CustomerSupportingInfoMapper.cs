using Dapper.FluentMap.Dommel.Mapping;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    public class CustomerSupportingInfoMapper : DommelEntityMap<CustomerSupportingInfo>
    {
        public CustomerSupportingInfoMapper()
        {
            ToTable(SqlTables.CustomerSupportingInfo);
            Map(p => p.Id).IsKey();
        }
    }
}

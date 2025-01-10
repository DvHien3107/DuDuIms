using Dapper.FluentMap.Dommel.Mapping;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    public class StoreBaseServiceEntityMapper : DommelEntityMap<StoreBaseService>
    {
        public StoreBaseServiceEntityMapper()
        {
            ToTable(SqlTables.StoreBaseService);
            Map(p => p.Id).IsKey().IsIdentity();
        }
    }
}

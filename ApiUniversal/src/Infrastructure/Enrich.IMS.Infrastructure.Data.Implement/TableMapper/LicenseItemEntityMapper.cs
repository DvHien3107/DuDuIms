using Dapper.FluentMap.Dommel.Mapping;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    public class LicenseItemEntityMapper : DommelEntityMap<LicenseItem>
    {
        public LicenseItemEntityMapper()
        {
            ToTable(SqlTables.LicenseItem);
            Map(p => p.ID).IsKey();
        }
    }
}

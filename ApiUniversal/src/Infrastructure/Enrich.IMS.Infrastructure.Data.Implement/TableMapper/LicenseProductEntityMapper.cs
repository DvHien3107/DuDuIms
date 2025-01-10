using Dapper.FluentMap.Dommel.Mapping;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    public class LicenseProductEntityMapper : DommelEntityMap<LicenseProduct>
    {
        public LicenseProductEntityMapper()
        {
            ToTable(SqlTables.LicenseProduct);
            Map(p => p.Id).IsKey();
        }
    }
}

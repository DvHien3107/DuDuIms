using Dapper.FluentMap.Dommel.Mapping;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    public class SystemConfigurationEntityMapper : DommelEntityMap<SystemConfiguration>
    {
        public SystemConfigurationEntityMapper()
        {
            ToTable(SqlTables.SystemConfiguration);
            Map(p => p.Id).IsKey();
        }
    }
}

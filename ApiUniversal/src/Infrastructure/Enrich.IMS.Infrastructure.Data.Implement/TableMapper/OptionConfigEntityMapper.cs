using Dapper.FluentMap.Dommel.Mapping;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    public class OptionConfigEntityMapper : DommelEntityMap<OptionConfig>
    {
        public OptionConfigEntityMapper()
        {
            ToTable(SqlTables.OptionConfig);
            Map(p => p.Id).IsKey();
        }
    }
}

using Dapper.FluentMap.Dommel.Mapping;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    public class RecurringHistoryEntityMapper : DommelEntityMap<RecurringHistory>
    {
        public RecurringHistoryEntityMapper()
        {
            ToTable(SqlTables.RecurringHistory);
            Map(p => p.Id).IsKey();
        }
    }
}

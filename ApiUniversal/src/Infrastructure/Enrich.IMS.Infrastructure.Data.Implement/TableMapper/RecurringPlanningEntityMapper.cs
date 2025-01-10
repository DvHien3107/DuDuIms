using Dapper.FluentMap.Dommel.Mapping;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    public class RecurringPlanningEntityMapper : DommelEntityMap<RecurringPlanning>
    {
        public RecurringPlanningEntityMapper()
        {
            ToTable(SqlTables.RecurringPlanning);
            Map(p => p.Id).IsKey();
        }
    }
}

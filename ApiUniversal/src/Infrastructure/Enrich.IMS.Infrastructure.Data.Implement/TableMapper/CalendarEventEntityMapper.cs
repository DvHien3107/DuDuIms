using Dapper.FluentMap.Dommel.Mapping;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    public class CalendarEventEntityMapper : DommelEntityMap<CalendarEvent>
    {
        public CalendarEventEntityMapper()
        {
            ToTable(SqlTables.CalendarEvent);
            Map(p => p.Id).IsKey();
        }
    }
}

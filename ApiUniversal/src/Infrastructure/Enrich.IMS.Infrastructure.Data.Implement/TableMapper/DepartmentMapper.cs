using Dapper.FluentMap.Dommel.Mapping;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    public class DepartmentMapper : DommelEntityMap<Department>
    {
        public DepartmentMapper()
        {
            ToTable(SqlTables.Department);
            Map(p => p.Id).IsKey();
        }
    }
}

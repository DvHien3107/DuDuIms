using Dapper.FluentMap.Dommel.Mapping;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    public class NewCustomerGoalEntityMapper : DommelEntityMap<NewCustomerGoal>
    {
        public NewCustomerGoalEntityMapper()
        {
            ToTable(SqlTables.NewCustomerGoal);
            Map(p => p.Id).IsKey().IsIdentity();
        }
    }
}

using Dapper.FluentMap.Dommel.Mapping;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    public class CustomerTransactionEntityMapper : DommelEntityMap<CustomerTransaction>
    {
        public CustomerTransactionEntityMapper()
        {
            ToTable(SqlTables.CustomerTransaction);
            Map(p => p.Id).IsKey();
        }
    }
}

using Dapper.FluentMap.Dommel.Mapping;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    public class CustomerGiftcardEntityMapper : DommelEntityMap<CustomerGiftcard>
    {
        public CustomerGiftcardEntityMapper()
        {
            ToTable(SqlTables.CustomerGiftcard);
            Map(p => p.Id).IsKey();
        }
    }
}

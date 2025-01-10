using Dapper.FluentMap.Dommel.Mapping;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    public class TicketTypeMappingEntityMapper : DommelEntityMap<TicketTypeMapping>
    {
        public TicketTypeMappingEntityMapper()
        {
            ToTable(SqlTables.TicketTypeMapping);
            Map(p => p.Id).IsKey().IsIdentity();
        }
    }
}

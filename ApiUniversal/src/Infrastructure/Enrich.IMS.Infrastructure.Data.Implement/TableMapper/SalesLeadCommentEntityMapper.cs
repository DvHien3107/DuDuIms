using Dapper.FluentMap.Dommel.Mapping;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    public class SalesLeadCommentEntityMapper : DommelEntityMap<SalesLeadComment>
    {
        public SalesLeadCommentEntityMapper()
        {
            ToTable(SqlTables.SalesLeadComment);
            Map(p => p.Id).IsKey().IsIdentity();
        }
    }
}

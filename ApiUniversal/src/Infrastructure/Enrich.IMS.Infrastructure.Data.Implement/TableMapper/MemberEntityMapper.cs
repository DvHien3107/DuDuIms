using Dapper.FluentMap.Dommel.Mapping;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    public class MemberEntityMapper : DommelEntityMap<Member>
    {
        public MemberEntityMapper()
        {
            ToTable(SqlTables.Member);
            Map(p => p.Id).IsKey().IsIdentity();
        }
    }
}

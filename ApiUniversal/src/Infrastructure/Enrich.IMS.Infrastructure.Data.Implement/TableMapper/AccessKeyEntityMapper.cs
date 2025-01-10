using Dapper.FluentMap.Dommel.Mapping;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    public class AccessKeyEntityMapper : DommelEntityMap<AccessKey>
    {
        public AccessKeyEntityMapper()
        {
            ToTable(SqlTables.AccessKey);
            Map(p => p.Id).IsKey().IsIdentity();
        }
    }
}

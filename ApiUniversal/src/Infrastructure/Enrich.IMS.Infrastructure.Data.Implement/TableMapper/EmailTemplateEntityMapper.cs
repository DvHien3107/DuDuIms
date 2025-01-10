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
    public class EmailTemplateEntityMapper : DommelEntityMap<EmailTemplate>
    {
        public EmailTemplateEntityMapper()
        {
            ToTable(SqlTables.EmailTemplate);
            Map(p => p.Id).IsKey();
        }
    }
}

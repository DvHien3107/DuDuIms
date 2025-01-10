using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto.EmailTemplate
{
    public class EmailTemplateDto
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public long TemplateGroupId { get; set; }

        public string TemplateGroupName { get; set; }

        public string Content { get; set; }
    }
}

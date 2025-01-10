using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Entities
{
    public class EmailTemplate
    {
     
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public string Name { get; set; }

        public long TemplateGroupId { get; set; }

        public string TemplateGroupName { get; set; }

        public string Content { get; set; }
    }
}

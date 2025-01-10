using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.DataTransfer.EnrichUniversal.Ticket
{
    public class ProjectMilestoneDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ParentId { get; set; }
        public string ParentName { get; set; }
        public string Type { get; set; }
        public bool? Active { get; set; }
        public string Description { get; set; }
        public int? Order { get; set; }
        public string TicketType { get; set; }
        public System.DateTime? UpdateAt { get; set; }
        public string UpdateByNumber { get; set; }
        public string BuildInCode { get; set; }
    }
}

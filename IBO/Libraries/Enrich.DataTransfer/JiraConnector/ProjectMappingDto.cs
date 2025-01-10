using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.DataTransfer.JiraConnector
{
    public class ProjectMappingDto
    {
        public int Id { get; set; }
        public string IMSId { get; set; }
        public string IMSName { get; set; }
        public string JiraId { get; set; }
        public string JiraName { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Enrich.IMS.JiraConnector
{
    public class JiraType
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public List<JiraStatus> Statuses { get; set; }
    }
}

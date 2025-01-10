using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto.SalesLead
{
    public class SalesLeadUpdateResponse
    {
        public string CreatedId { get; set; }

        public SalesLeadDto SalesLead { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto.SalesLead
{
    public class SalesLeadUpdateOption : SalesLeadDetailLoadOption
    {
        public new static SalesLeadUpdateOption Default => new SalesLeadUpdateOption
        {
            Customer = true
        };
    }
}

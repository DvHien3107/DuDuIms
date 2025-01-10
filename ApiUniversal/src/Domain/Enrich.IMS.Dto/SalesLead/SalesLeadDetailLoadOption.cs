using Enrich.Dto.Base.Attributes;
using Enrich.IMS.Dto.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto.SalesLead
{
    public class SalesLeadDetailLoadOption : LoadOrUpdateBaseOption
    {
        public new static SalesLeadDetailLoadOption Default => new SalesLeadDetailLoadOption
        {
            Customer = true
        };

        [LoadOrUpdateOption(nameof(SalesLeadDto.Customer))]
        public bool Customer { get; set; }

    }
}

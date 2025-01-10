using Enrich.Common.Enums;
using Enrich.Common.Helpers;
using Enrich.Dto.Base.Attributes;
using System;

namespace Enrich.IMS.Dto
{    
    public partial class SalesLeadDto
    {
        public CustomerDto Customer { get; set; }
    }
}

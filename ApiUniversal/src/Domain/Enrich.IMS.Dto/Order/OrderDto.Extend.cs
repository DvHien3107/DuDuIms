using Enrich.Common.Enums;
using Enrich.Common.Helpers;
using Enrich.Dto.Base.Attributes;
using System;

namespace Enrich.IMS.Dto
{    
    public partial class OrderDto
    {
        public string StoreCode { get; set; }
        public string StoreName { get; set; }
    }
}

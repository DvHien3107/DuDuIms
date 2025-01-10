using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enrich.DataTransfer
{
    public class CustomerSearchResponse : PagingResponseDto<CustomerDto>
    {
        public Summary Summary { get; set; }
    }
    public class Summary
    {
        public int? TotalStoreOfMerchant { get; set; }
        public int? TotalStoreInHouse { get; set; }
    }
}
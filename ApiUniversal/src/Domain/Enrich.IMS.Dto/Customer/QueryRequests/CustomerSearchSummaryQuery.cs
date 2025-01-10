using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto.Customer
{
    public class CustomerSearchSummaryQuery
    {
        public string QueryTotalStoreOfMerchant { get; set; }

        public string QueryTotalStoreInHouse { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto.Customer.QueryRequests.NewRequest
{
    public class ReportCustomerByMonthRqt
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string Type { get; set; }
    }
}

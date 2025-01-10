using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.DataTransfer.EnrichUniversalService.MerchantReport
{
    public class CustomerChartReportRequest : BaseSearchWithFilterRequest<CustomerChartReportCondition, CustomerChartReportFilterCondition>
    {
        public CustomerChartReportRequest()
        {
            Condition = new CustomerChartReportCondition();
        }
    }

    public class CustomerChartReportCondition
    {
        public string Type { get; set; }
        public string Unit { get; set; }
        public int Year { get; set; } = DateTime.UtcNow.Year;
    }

    public class CustomerChartReportFilterCondition
    {
    }
}

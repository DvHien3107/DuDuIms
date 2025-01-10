using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.DataTransfer.EnrichUniversalService.MerchantReport
{
    public class CustomerChartReportResponse
    {
        public int TotalCustomer { get; set; }
        public List<CustomerChartReportListItemDto> Records { get; set;}
    }

    public partial class CustomerChartReportListItemDto
    {
        public string ColumnName { get; set; }
        public int ColumnNumber { get; set; }
        public int NumberCustomer { get; set; }
        public int Goal { get; set; }
    }
}

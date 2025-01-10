using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel
{
    public partial class SalesPersonReportCustomizeModel
    {
        public string SalesPersonName { get; set; }
        public IEnumerable<DataSalesLeadForStatus> Data { get; set; }
    }
    public partial class DataSalesLeadForStatus
    {
        public string StatusName { get; set; }
        public int CountStatus { get; set; }
        public  IEnumerable<int> Data { get; set; }
    }

}
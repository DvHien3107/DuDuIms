using EnrichcousBackOffice.Models.UniversalApi.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.UniversalApi.TransactionReport
{
    public class TransactionReportResponse : PagingResponseDto<TransactionReport>
    {
      public Summary Summary { get; set; }
      public double? Income { get; set; }
    }
    public class Summary
    {
        public double? TotalAmount { get; set; }
       
    }
}
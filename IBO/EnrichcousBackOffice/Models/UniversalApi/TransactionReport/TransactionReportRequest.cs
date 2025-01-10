using EnrichcousBackOffice.Models.UniversalApi.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.UniversalApi.TransactionReport
{
    public class TransactionReportRequest : BaseSearchWithFilterRequest<TransactionReportSearchCondition, TransactionReportSearchFilterCondition>
    {
        public TransactionReportRequest()
        {
            Condition = new TransactionReportSearchCondition();
        }
    }
}
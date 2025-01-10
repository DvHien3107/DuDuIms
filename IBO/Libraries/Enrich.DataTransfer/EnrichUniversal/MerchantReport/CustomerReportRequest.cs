using System;
using System.Collections.Generic;

namespace Enrich.DataTransfer
{
    public class CustomerReportRequest
    {
        public CustomerReportCondition Condition { get; set; } = new CustomerReportCondition();
    }

    public partial class CustomerReportCondition
    {
        public bool IsActive { get; set; } = true;
        public bool OnlyGetSummaries { get; set; } = true;
    }
}

using Enrich.Dto.Base;
using Enrich.IMS.Dto.Common;
using System;
using System.Collections.Generic;

namespace Enrich.IMS.Dto.Customer
{
    public partial class CustomerReportCondition : BaseSearchCondition
    {
        public DateTime FromDate { get; set; } = new DateTime(2000, 1, 1);
        public DateTime ToDate { get; set; } = new DateTime(3000, 1, 1);
        public bool IsActive { get; set; } = true;
        public bool OnlyGetSummaries { get; set; }

        /// <summary>
        /// Dont use
        /// </summary>
        public List<KeyValueDto<string>> OnStringFields { get; set; }
    }
}
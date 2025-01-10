using Enrich.Common.Enums;
using Enrich.Dto.Base;
using Enrich.IMS.Dto.Common;
using System;
using System.Collections.Generic;

namespace Enrich.IMS.Dto.Customer
{
    public partial class CustomerChartReportCondition : BaseSearchCondition
    {
        public string Type { get; set; }
        public string Unit { get; set; } = TimeUnit.Month.ToString();
        public int Year { get; set; } = DateTime.UtcNow.Year;

        /// <summary>
        /// Dont use
        /// </summary>
        public List<KeyValueDto<string>> OnStringFields { get; set; }
    }
}
using Enrich.Dto.Base;
using Enrich.IMS.Dto.Common;
using System;
using System.Collections.Generic;

namespace Enrich.IMS.Dto.NewCustomerGoal
{
    public partial class NewCustomerGoalSearchCondition : BaseSearchCondition
    {
        /// custom for sale lead dashboard
        public int Year { get; set; }
        public int Month { get; set; }

        /// <summary>
        /// Dont use
        /// </summary>
        public List<KeyValueDto<string>> OnStringFields { get; set; }
    }
}

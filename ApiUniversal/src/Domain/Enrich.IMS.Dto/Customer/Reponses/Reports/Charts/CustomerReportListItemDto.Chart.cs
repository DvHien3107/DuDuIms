using Enrich.Common;
using Enrich.Common.Enums;
using Enrich.Common.Helpers;
using Enrich.Dto.Attributes;
using Enrich.Dto.Base;
using System;

namespace Enrich.IMS.Dto.Customer
{
    public partial class CustomerChartReportListItemDto : ListItemDto
    {
        /// <summary>
        ///month January, February, March, April, May, June, July, August, September, October, November, December
        ///week Week 1, Week 2, Week 3, Week 4,..., Week 52
        /// </summary>
        [GridField(Index = 1, ColumnName = "MonthName", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        public string ColumnName { get; set; }

        /// <summary>
        ///month 1, 2, 3, 4,..., 12
        ///week 1, 2, 3, 4,..., 52
        /// </summary>
        [GridField(Index = 1, ColumnName = "MonthNumber", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        public int ColumnNumber { get; set; }


        [GridField(Index = 1, ColumnName = "NumberCustomer", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        public int NumberCustomer { get; set; }


        [GridField(Index = 1, ColumnName = "Goal", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        public int Goal { get; set; }
    }
}
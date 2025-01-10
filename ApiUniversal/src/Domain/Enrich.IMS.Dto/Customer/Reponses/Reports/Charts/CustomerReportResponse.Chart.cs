using Enrich.Dto.List;
using System;

namespace Enrich.IMS.Dto.Customer
{
    public class CustomerChartReportResponse : PagingResponseDto<CustomerChartReportListItemDto>
    {
        public int TotalCustomer { get; set; }
    }
    public class ObjectResponse : PagingResponseDto<dynamic>
    {
        public int TotalCustomer { get; set; }
    }
}

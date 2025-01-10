using Enrich.Dto.List;

namespace Enrich.IMS.Dto.Customer
{
    public class CustomerReportResponse : PagingResponseDto<CustomerReportListItemDto>
    {
        public CustomerReportSummary Summary { get; set; } = new CustomerReportSummary();
    }
}

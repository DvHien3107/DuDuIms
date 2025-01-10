using Enrich.Dto.List;

namespace Enrich.IMS.Dto.Customer
{
    public class CustomerSearchResponse : PagingResponseDto<CustomerListItemDto>
    {
        public CustomerSearchSummary Summary { get; set; } = new CustomerSearchSummary();
    }
}

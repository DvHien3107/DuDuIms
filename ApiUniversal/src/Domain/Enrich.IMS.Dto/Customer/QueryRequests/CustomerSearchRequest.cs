using Enrich.Dto.Base.Requests;

namespace Enrich.IMS.Dto.Customer
{
    public class CustomerSearchRequest : BaseSearchWithFilterRequest<CustomerSearchCondition, CustomerFilterCondition>
    {
        public CustomerSearchSummaryQuery SummaryQuery { get; set; } = new CustomerSearchSummaryQuery();
    }
}
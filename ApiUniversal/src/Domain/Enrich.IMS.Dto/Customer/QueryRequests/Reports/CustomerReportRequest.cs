using Enrich.Dto.Base.Requests;

namespace Enrich.IMS.Dto.Customer
{
    public class CustomerReportRequest : BaseSearchWithFilterRequest<CustomerReportCondition, CustomerReportFilterCondition>
    {
        public CustomerReportSummaryQuery SummaryQuery { get; set; } = new CustomerReportSummaryQuery();
    }
}
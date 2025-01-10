using Enrich.DataTransfer;
using Enrich.DataTransfer.EnrichUniversalService.MerchantReport;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IServices.Utils.Universal
{
    public interface IEnrichUniversalService
    {
        Task<string> GetRecurringCardAsync(string customerCode);
        Task<CustomerSearchResponse> GetMerchantFromUniversal(CustomerSearchRequest request);

        Task<CustomerReportSummary> GetMerchantSummariesAsync();

        Task<object> GetLookupDataAsync(string type);

        Task<object> ReportCustomerForChartAsync(CustomerChartReportRequest request);

        Task InitialStoreDataAsync(string storeCode);

        Task<bool> SyncingSalesLeadFromMondayAsync();
        EnrichContext readContext();
    }
}

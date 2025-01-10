using Enrich.DataTransfer;
using Enrich.DataTransfer.EnrichSMSService;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IServices.Utils.SMS
{
    public interface IEnrichSMSService
    {
        //Task InitialStoreDataAsync(string storeCode);

        Task InsertHistorySMSRemainingAsync(SMSHistoryRemaining request);

        Task<HistoryTimeLineResponse> GetTimeLineAsync(HistoryTimeLineRequest request);
    }
}

using Enrich.Dto.Base;
using Enrich.IMS.Dto.EnrichSMSService;
using System.Threading.Tasks;

namespace Enrich.Core.Utils
{
    public interface IEnrichSMSService
    {
        Task InsertHistorySMSRemainingAsync(SMSHistoryRemaining request);

        /// <summary>
        /// Search history async from Enrich SMS Service
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<HistorySearchResponse> SearchHistoryAsync(HistorySearchRequest request);

        /// <summary>
        /// Get timeline history async from Enrich SMS Service
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<HistoryTimeLineResponse> TimeLineHistoryAsync(HistoryTimeLineRequest request);
    }
}

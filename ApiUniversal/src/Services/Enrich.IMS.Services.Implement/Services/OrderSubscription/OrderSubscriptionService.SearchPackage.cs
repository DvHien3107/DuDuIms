using Enrich.Dto.Base;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.OrderSubscription;
using Enrich.Infrastructure.Data.Dapper.Library;
using System.Linq;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Services
{
    public partial class OrderSubscriptionService
    {
        /// <summary>
        /// Report order subscription
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<SubscriptionPackageSearchResponse> SearchSubscriptionPackageAsync(string package, SubscriptionPackageSearchRequest request)
        {
            request.Condition.Package = package;
            var response = await _repository.SearchSubscriptionPackageAsync(request);
            return response;
        }
    }
}

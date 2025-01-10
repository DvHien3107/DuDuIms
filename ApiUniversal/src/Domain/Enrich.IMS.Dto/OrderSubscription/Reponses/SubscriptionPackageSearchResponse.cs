using Enrich.Dto.List;
using System.Collections.Generic;

namespace Enrich.IMS.Dto.OrderSubscription
{
    public class SubscriptionPackageSearchResponse
    {
        public IEnumerable<SubscriptionPackageListItemDto> Records { get; set; }
    }
}

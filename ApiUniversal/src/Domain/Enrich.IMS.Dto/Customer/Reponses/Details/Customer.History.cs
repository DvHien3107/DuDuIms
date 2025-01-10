using System.Collections.Generic;

namespace Enrich.IMS.Dto.Customer
{
    public partial class CustomerDetailResponse
    {
        public IEnumerable<OrderDto> Orders { get; set; }
        public IEnumerable<OrderSubscriptionDto> Subscriptions { get; set; }
    }
}
using Enrich.Dto.List;

namespace Enrich.IMS.Dto.OrderSubscription
{
    public class OrderSubscriptionSearchResponse : PagingResponseDto<OrderSubscriptionListItemDto>
    {
        public OrderSubscriptionSearchSummary Summary { get; set; } = new OrderSubscriptionSearchSummary();
        public decimal Income {  get; set; } = decimal.Zero;
    }
}

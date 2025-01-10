using Enrich.IMS.Dto;

namespace Enrich.BusinessEvents.IMS
{
    public class OrderPaymentEvent<TValue>
    {
        public TValue Value { get; set; }
    }
}
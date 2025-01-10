
namespace Enrich.BusinessEvents.IMS
{
    public class OrderPaymentFailedEvent
    {
        /// <summary>
        /// Transaction Id
        /// </summary>
        public string TransactionId { get; set; }

        /// <summary>
        /// Order Code
        /// </summary>
        public string OrderCode { get; set; }

        /// <summary>
        /// Customer Id
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// Order Id
        /// </summary>
        public string OrderId { get; set; }

        /// <summary>
        /// Total Amount
        /// </summary>
        public decimal GrandTotal { get; set; }

        /// <summary>
        /// Payment Note
        /// </summary>
        public string PaymentType { get; set; }

        /// <summary>
        /// Response Text
        /// </summary>
        public string ResponseText { get; set; }


        /// <summary>
        /// Store Code
        /// </summary>
        public string StoreCode { get; set; }

        /// <summary>
        /// Customer Name
        /// </summary>
        public string StoreName { get; set; }
    }
}
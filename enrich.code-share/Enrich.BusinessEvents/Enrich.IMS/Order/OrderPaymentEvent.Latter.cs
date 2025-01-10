using System;

namespace Enrich.BusinessEvents.IMS
{
    public class OrderPaymentLaterEvent
    {
        /// <summary>
        /// Order Id
        /// </summary>
        public string OrderId { get; set; }

        /// <summary>
        /// Customer Id
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// Order Code
        /// </summary>
        public string OrderCode { get; set; }

        /// <summary>
        /// Grand Total
        /// </summary>
        public decimal GrandTotal { get; set; }

        /// <summary>
        /// Customer Name
        /// </summary>
        public string StoreName { get; set; }

        /// <summary>
        /// Store Code
        /// </summary>
        public string StoreCode { get; set; }

        /// <summary>
        /// Due Date
        /// </summary>
        public DateTime? DueDate { get; set; }
    }
}
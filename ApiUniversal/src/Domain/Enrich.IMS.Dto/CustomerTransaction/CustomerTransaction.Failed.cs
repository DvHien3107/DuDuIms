using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto.CustomerTransaction
{
    public class CustomerTransactionFailed
    {
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
        public decimal Amount { get; set; }

        /// <summary>
        /// Payment Note
        /// </summary>
        public string PaymentNote { get; set; }

        /// <summary>
        /// Response Text
        /// </summary>
        public string ResponseText { get; set; }

        /// <summary>
        /// Customer Name
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// Store Code
        /// </summary>
        public string StoreCode { get; set; }

        /// <summary>
        /// Card Holder Name
        /// </summary>
        public string CardHolderName { get; set; }

        /// <summary>
        /// Card Number
        /// </summary>
        public string CardNumber { get; set; }

        /// <summary>
        /// Card Type
        /// </summary>
        public string CardType { get; set; }

    }
}

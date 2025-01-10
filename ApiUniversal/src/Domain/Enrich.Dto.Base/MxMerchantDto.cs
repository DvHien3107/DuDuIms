using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Dto.Base
{
    public class MxMerchantDto
    {
        public string MxMerchantId { get; set; }
        public string MxMerchantToken { get; set; }
        public string AccountNumber { get; set; }
        public string RoutingNumber { get; set; }
        public string OwnerName { get; set; }
        public string BankName { get; set; }
        public decimal Amount { get; set; }
    }
    public class MxMerchantResponseDto
    {
        public string id { get; set; }
        public string paymentToken { get; set; }
        public string status { get; set; }
        public string authMessage { get; set; }
    }
    public class receipt_data
    {
        public string salon;
        public string card;
        public string name;
        public string order_code;
        public string date_paid;
        public string total;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnrichcousBackOffice.Models.Proc
{
    public class P_LoadOrderDetail
    {
        public string OrderCode { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public decimal? Price { get; set; }
        public int? Quantity { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? Discount { get; set; }
        public string SubscriptionType { get; set; }
    }
}

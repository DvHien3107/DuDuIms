using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel
{

    public class BaseServicePackage
    {
        public string StoreCode { get; set; }
        public string SubscriptionCode { get; set; }
        public double Price { get; set; }
        public List<BaseServiceQuantity> baseServices { get; set; } = new List<BaseServiceQuantity> { };
    }
    public class BaseServiceQuantity
    {
        public string Code { get; set; }
        public int Quantity { get; set; }
    }
}
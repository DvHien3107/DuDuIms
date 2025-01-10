using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel
{
    public class CurrentLicenseIsActivedCustomizeModel
    {
        public string StoreServiceId { get; set; }
        public string ProductCode { get; set; }

        public string ProductName { get; set; }
        public decimal Price { get; set; }  
        public DateTime? RenewDate { get; set; }
        public int? Status { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public string Expires { get; set; }
        public string Type { get; set; }
    }
}
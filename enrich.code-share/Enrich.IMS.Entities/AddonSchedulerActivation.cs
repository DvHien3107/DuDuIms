using System;

namespace Enrich.IMS.Entities
{   
    public partial class AddonSchedulerActivation
    {
        public string Id { get; set; }
        public string CustomerCode { get; set; }
        public string StoreCode { get; set; }
        public string StoreName { get; set; }
        public string OrderCode { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public int? Status { get; set; }
        public string StoreServiceId { get; set; }
    }
}

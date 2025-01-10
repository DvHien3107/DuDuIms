using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnrichcousBackOffice.Models.Proc
{
    public class ModelLoadRecurringPlan
    {
        public int planningId { get; set; }
        public string customerStoreCode { get; set; }
        public string customerCustomerCode { get; set; }
        public string customerBusinessName { get; set; }
        public string planningOrderCode { get; set; }
        public string planningSubscriptionCode { get; set; }
        public DateTime? planningRecurringDate { get; set; }
        public int? planningStatus { get; set; }
        public string licenseCode { get; set; }
    }
}

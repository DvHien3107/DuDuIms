
using Enrich.Dto.Base;
using System.Collections.Generic;

namespace Enrich.IMS.Dto.Customer
{
    public class CustomerReportSummary
    {
        public int TotalMerchant { get; set; }

        public int TotalMerchantLicenseAndMID { get; set; }

        public int TotalMerchantLicense { get; set; }

        public int TotalMerchantMID { get; set; }

        public int TotalMerchantPendingDelivery { get; set; }

        public int TotalMerchantLive { get; set; }
    }
}

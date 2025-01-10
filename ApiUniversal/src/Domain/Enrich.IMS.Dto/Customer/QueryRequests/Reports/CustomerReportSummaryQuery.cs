
using Enrich.Dto.Base;
using System.Collections.Generic;

namespace Enrich.IMS.Dto.Customer
{
    public class CustomerReportSummaryQuery
    {
        public string QueryTotalMerchant { get; set; }

        public string QueryTotalMerchantLicenseAndMID { get; set; }

        public string QueryTotalMerchantLicense { get; set; }

        public string QueryTotalMerchantMID { get; set; }

        public string QueryTotalMerchantPendingDelivery { get; set;}

        public string QueryTotalMerchantLive { get; set; }
    }
}

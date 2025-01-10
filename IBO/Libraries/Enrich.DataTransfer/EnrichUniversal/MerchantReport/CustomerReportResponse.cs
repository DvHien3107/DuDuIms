using System;

namespace Enrich.DataTransfer
{
    public class CustomerReportResponse
    {
        public CustomerReportSummary Summary { get; set; } = new CustomerReportSummary();
    }

    public class CustomerReportSummary
    {
        public int TotalMerchant { get; set; } = 0;

        public int TotalMerchantLicenseAndMID { get; set; } = 0;

        public int TotalMerchantLicense { get; set; } = 0;

        public int TotalMerchantMID { get; set; } = 0;
    }
}

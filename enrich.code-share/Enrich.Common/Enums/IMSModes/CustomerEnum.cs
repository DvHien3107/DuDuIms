using System.ComponentModel.DataAnnotations;

namespace Enrich.Common.Enums
{
    public class CustomerEnum
    {
        public enum StatusSearch
        {
            [Display(Name = "Expires")] Expires = -1, // search from store_service
            [Display(Name = "DeActivated")] Deactive = 0, // search from store_service
            [Display(Name = "Activated")] Active = 1, // search from store_service
            [Display(Name = "Cancle")] Cancle = 2, // search from cancle date
        }

        public enum MerchantStatusSearch
        {
            [Display(Name = "All")] All = 0, // store service (active = 1) or ( (terminal status is null or empty) and MID is not null) 
            [Display(Name = "Live merchant (terminal or license)")] LiveMerchant = 1, // store service (active = 1) or ( (terminal status is null or empty) and MID is not null) 
            [Display(Name = "License only (live)")] OnlyLicense = 2, // store service (active = 1) and renew date < GETUTCDATE
            [Display(Name = "Terminal only (live)")] OnlyTerminal = 3, // store service (active = 1) and renew date < GETUTCDATE
            [Display(Name = "License and terminal (live)")] LicenseTerminal = 4, // store service (active = 1) and renew date < GETUTCDATE
            [Display(Name = "Expired license and active terminal")] ExpiredLicenseActiveTerminal = 5, // store service (active = 1) and renew date < GETUTCDATE
            [Display(Name = "Expired license and inactive terminal")] ExpiredLicenseInactiveTerminal = 6, // store service (active = 1) and renew date < GETUTCDATE
            [Display(Name = "Inactive license and inactive terminal")] InactiveLicenseInactiveTerminal = 7, // store service (active = 1) and renew date < GETUTCDATE
            [Display(Name = "Canceled merchant")] Cancel = 8, // search from cancle date
        }

        public enum ServiceType
        {
            [Display(Name = "Trial account", Description = "Trial account")] Merchant_Trial,
            [Display(Name = "Merchant with License", Description = "Software Only")] Merchant_License,
            [Display(Name = "Merchant with MID", Description = "Terminal Only")] Merchant_MID,
            [Display(Name = "Merchant with License and MID", Description = "Software and Terminal")] Merchant_License_MID
        }

        /// <summary>
        /// use for report
        /// </summary>
        public enum AccountStatus
        {
            [Display(Name = "Live")] Live,
            [Display(Name = "Processing")] Processing,
            [Display(Name = "Live and Processing")] LiveNProcessing,
            [Display(Name = "Pending Delivery")] PendingDelivery
        }

        public enum CustomerType
        {
            STORE_IN_HOUSE = 0,
            STORE_OF_MERCHANT = 1,
        }

        public enum ReportType
        {
            [Display(Name = "active-account")] Active,
            [Display(Name = "canceled-account")] Canceled
        }

        public enum ReportChartType
        {
            [Display(Name = "chart-new-customer")] NewCustomer,
            [Display(Name = "chart-cancel-customer")] Cancel
        }

        public enum NODaysCreatedSearch
        {
            All = 0,
            [Display(Name = "7 days")] Day_7,
            [Display(Name = "14 days")] Day_14,
            [Display(Name = "1 month")] Month_1,
            [Display(Name = "2 months")] Month_2,
            [Display(Name = "3 months")] Month_3,
            [Display(Name = "4 months")] Month_4,
            [Display(Name = "6 months")] Month_6,
            [Display(Name = "12 months")] Month_12,
            [Display(Name = "Day custom")] Custom
        }

        public enum AtRiskSearch
        {

            [Display(Name = "Expiring in 3 days and no card on profile")] Expried_In_3Day,
            [Display(Name = "Have not process credit card")] Dont_Have_Card
        }

        public enum RemainingDaySearch
        {
            [Display(Name = "All")] All = 0,
            [Display(Name = "7 days")] Day_7,
            [Display(Name = "14 days")] Day_14,
            [Display(Name = "1 month")] Month_1,
            [Display(Name = "2 months")] Month_2,
            [Display(Name = "3 months")] Month_3,
            [Display(Name = "4 months")] Month_4,
            [Display(Name = "6 months")] Month_6,
            [Display(Name = "12 months")] Month_12,
            [Display(Name = "Day custom")] Custom = 100
        }

        /// <summary>
        /// Tab in merchant detail
        /// </summary>
        public enum TabName
        {
            [Display(Name = "Support Overview")] Support,
            [Display(Name = "Profile")] Profile,
            [Display(Name = "Current plan")] Service,
            [Display(Name = "Transaction")] Transaction,
            [Display(Name = "History")] History,
            [Display(Name = "Other")] Other,
        }
        /// <summary>
        /// Merchant option
        /// </summary>
        public sealed class MerchantOptionNameSpaceEnum
        {
            public const string Processor = "Merchant.Processor";
            public const string Source = "Merchant.Source";
            public const string TerminalType = "Merchant.TerminalType";
            public const string DeviceName = "Merchant.DeviceName";
            public const string POSStructure = "Merchant.POSStructure";
            public const string DeviceSetupStructure = "Merchant.DeviceSetupStructure";
        }
    }
}

using System;

namespace Enrich.IMS.Entities
{
    public partial class SystemConfiguration
    {
        public int Id { get; set; }
        public string AdminEmail { get; set; }
        public string SupportEmail { get; set; }
        public string SalesEmail { get; set; }
        public string CompanyName { get; set; }
        public string Logo { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyHotline { get; set; }
        public string CompanyFax { get; set; }
        public string NotificationEmail { get; set; }
        public string HREmail { get; set; }
        public string ITEmail { get; set; }
        public System.DateTime? Last_scan_date { get; set; }
        public string Adobe_refresh_token { get; set; }
        public string UPS_ShipperAddress { get; set; }
        public string UPS_ShipperCellPhone { get; set; }
        public string UPS_ShipAttentionName { get; set; }
        public string UPS_ShipperAccountNumber { get; set; }
        public string UPS_AccountName { get; set; }
        public string UPS_AccountPassword { get; set; }
        public string UPS_AccessLicenseNumber { get; set; }
        public string UPS_AccountNumber { get; set; }
        public string CompanySupportNumber { get; set; }
        public string DocuSign_refresh_token { get; set; }
        public string DocuSign_access_token { get; set; }
        public string MxMerchant_RequestToken { get; set; }
        public string MxMerchant_RequestSecret { get; set; }
        public string MxMerchant_AccessToken { get; set; }
        public string MxMerchant_AccessSecret { get; set; }
        public bool? AutoActiveRecurringLicense { get; set; }
        public string BillingNotification { get; set; }
        public string ProductName { get; set; }
        public string ProductLogo { get; set; }
        public bool? ActiveOnboardingTicket { get; set; }
        public int? NotificationBeforeExpiration { get; set; }
        public int? RecurringBeforeDueDate { get; set; }
        public string MerchantPasswordDefault { get; set; }
        public bool EnableSendRecurringEmailToBilling { get; set; }
        public int? ExtensionDay { get; set; }

        public int? GoalScore { get; set; }
    }
}

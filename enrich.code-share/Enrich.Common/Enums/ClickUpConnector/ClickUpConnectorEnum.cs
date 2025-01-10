using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Enrich.Common.Enums
{
    public class ClickUpConnectorEnum
    {
        public enum MapType
        {
            MerchantField = 100,
            MerchantCustomField = 110,
            OnboardingDeliveryField = 200,
            OnboardingDeliveryCustomField = 210
        }

        public enum TaskType
        {
            Merchant = 100,
            Delivery = 200
        }

        public enum EventType
        {
            merchant_created,
            task_update,
            invoice_created
        }

        public sealed class CustomFieldType
        {
            public const string Email = "email";

            public const string ShortText = "short_text";

            public const string Phone = "phone";

            public const string DropDown = "drop_down";

            public const string Currency = "currency";

            public const string Date = "date";

            public const string AutomaticProgress = "automatic_progress";

            public const string Users = "users";

            public const string Text = "text";

            public const string Url = "url";
        }

        public sealed class MerchantStatus
        {
            public const string CanceledAccount = "Canceled account";

            public const string TerminalOnly = "Terminal Only";

            public const string SoftwareOnly = "Software Only";

            public const string SoftwareAndTerminal = "Software and Terminal";
        }

        public sealed class MerchantTerminalStatus
        {
            public const string Active = "Active";

            public const string Inactive = "Inactive";
        }

        public sealed class MerchantCustomField
        {
            public const string SalonAddress1 = "SalonAddress1";

            public const string SalonCity = "SalonCity";

            public const string SalonState = "SalonState";

            public const string SalonZipcode = "SalonZipcode";

            public const string BusinessCountry = "BusinessCountry";

            public const string StoreCode = "StoreCode";

            public const string SalonEmail = "SalonEmail";

            public const string SalonPhone = "SalonPhone";

            public const string MangoEmail = "MangoEmail";

            public const string MID = "MID";

            public const string SalonTimeZone = "SalonTimeZone";

            public const string GoLiveDate = "GoLiveDate";

            public const string OwnerName = "OwnerName";

            public const string CellPhone = "CellPhone";

            public const string Email = "Email";

            public const string BusinessDescription = "BusinessDescription";

            public const string City = "City";

            public const string State = "State";

            public const string Zipcode = "Zipcode";

            public const string OwnerAddressStreet = "OwnerAddressStreet";

            public const string OwnerAddressCivicNumber = "OwnerAddressCivicNumber";

            public const string SalonAddress2 = "SalonAddress2";

            public const string Country = "Country";

            public const string PreferredName = "PreferredName";

            public const string PreferredLanguage = "PreferredLanguage";

            public const string POSStructureName = "POSStructureName";

            public const string DepositAccountNumber = "DepositAccountNumber";

            public const string DepositBankName = "DepositBankName";

            public const string DepositRoutingNumber = "DepositRoutingNumber";

            public const string LegalName = "LegalName";

            public const string TerminalTypeName = "TerminalTypeName";

            public const string DeviceNote = "DeviceNote";

            public const string DeviceName = "DeviceName";

            public const string Website = "Website";

            public const string SourceName = "SourceName";

            public const string DeviceSetupStructureName = "DeviceSetupStructureName";

            public const string ProcessorName = "ProcessorName";

            public const string CancelDate = "CancelDate";

            public const string ManagerPhone = "ManagerPhone";

            public const string LicenseName = "LicenseName";

            public const string TerminalStatusName = "TerminalStatusName";

            public const string SalesPersonEmail = "SalesPersonEmail";

            public const string MemberNumberEmail = "MemberNumberEmail";
        }

        public sealed class WebhookEvent
        {
            public const string TaskUpdated = "taskUpdated";
        }

        public enum ChangeType
        {
            Added,
            Removed,
            Modified
        }
    }
}
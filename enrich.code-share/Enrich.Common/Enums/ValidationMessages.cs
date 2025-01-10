using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Enrich.Common.Enums
{
    public class ValidationMessages
    {
        public sealed class IsExist
        {
            public const string SalonEmail = "Salon Email already exists.";
        }
        public sealed class Invalid
        {
            public const string StoreService = "Store service invalid";
            public const string Action = "Activator action invalid";
            public const string RecurringPlanning = "Recurring planning data invalid";
            public const string Customer = "Customer data invalid";
            public const string Order = "Order data invalid";
            public const string OrderSubscription = "Order subscription data invalid";
            public const string BillingEmail = "Billing email invalid";
        }
        public sealed class NotFound
        {
            public const string StoreService = "Store service data not found";
            public const string Customer = "Merchant data not found";
            public const string Order = "Order data not found";
            public const string Card = "Credit card data not found";
            public const string MxMerchant = "MxMerchant Id data not found";
            public const string ConfigLastScan = "Last scan config not found";
            public const string ConfigPeriodScan = "Period scan config not found";
            public const string EmailConfig = "Email config not found";
            public const string SalesLead = "Sales lead data not found";
            public const string Ticket = "Ticket data not found";
            public const string BaseService = "Base service not found";
        }
        public sealed class NotDefined
        {
            public const string OrderStatus = "Status input is not defined";
        }
        public sealed class NotYet
        {
            public const string Card = "Customer not yet credit card";
        }
    }
}
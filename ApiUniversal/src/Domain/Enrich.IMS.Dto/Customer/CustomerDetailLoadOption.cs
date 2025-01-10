using Enrich.Dto.Base.Attributes;
using Enrich.IMS.Dto.Common;

namespace Enrich.IMS.Dto.Customer
{
    public class CustomerDetailLoadOption : LoadOrUpdateBaseOption
    {
        public static CustomerDetailLoadOption Default => new CustomerDetailLoadOption
        {
            Information = true,
            SupportingInfo = true,
            Comment = true,
            Log = true
        };
        public static CustomerDetailLoadOption GetSupport(bool information = true) => new CustomerDetailLoadOption
        {
            Information = information,
            SupportingInfo = true,
            Comment = true,
            Log = true
        };
        public static CustomerDetailLoadOption GetProfile(bool information = true) => new CustomerDetailLoadOption
        {
            Information = information,
            Profile = true,
            Contact = true
        };

        public static CustomerDetailLoadOption GetService(bool information = true) => new CustomerDetailLoadOption
        {
            Information = information,
            StoreService = true,
            RecurringPlanning = true
        };

        public static CustomerDetailLoadOption GetTransaction(bool information = true) => new CustomerDetailLoadOption
        {
            Information = information,
            Transaction = true,
            Card = true
        };

        public static CustomerDetailLoadOption GetHistory(bool information = true) => new CustomerDetailLoadOption
        {
            Information = information,
            Order = true,
            Subscription = true
        };

        public static CustomerDetailLoadOption GetOther(bool information = true) => new CustomerDetailLoadOption
        {
            Information = information,
            File = true
        };

        #region [Detail]
        [LoadOrUpdateOption(nameof(CustomerDetailResponse.Information))]
        public bool Information { get; set; }
        #endregion

        #region [Support Overview]
        [LoadOrUpdateOption(nameof(CustomerDetailResponse.SupportingInfos))]
        public bool SupportingInfo { get; set; }
        [LoadOrUpdateOption(nameof(CustomerDetailResponse.Comments))]
        public bool Comment { get; set; }
        [LoadOrUpdateOption(nameof(CustomerDetailResponse.Logs))]
        public bool Log { get; set; }
        #endregion

        #region [Profile]
        [LoadOrUpdateOption(nameof(CustomerDetailResponse.Profile))]
        public bool Profile { get; set; }
        [LoadOrUpdateOption(nameof(CustomerDetailResponse.Contacts))]
        public bool Contact { get; set; }
        #endregion

        #region [Current plan]
        [LoadOrUpdateOption(nameof(CustomerDetailResponse.StoreServices))]
        public bool StoreService { get; set; }
        public bool RecurringPlanning { get; set; }
        #endregion

        #region [Transaction]
        [LoadOrUpdateOption(nameof(CustomerDetailResponse.Transactions))]
        public bool Transaction { get; set; }
        [LoadOrUpdateOption(nameof(CustomerDetailResponse.Cards))]
        public bool Card { get; set; }
        #endregion

        #region [History]
        [LoadOrUpdateOption(nameof(CustomerDetailResponse.Orders))]
        public bool Order { get; set; }
        [LoadOrUpdateOption(nameof(CustomerDetailResponse.Subscriptions))]
        public bool Subscription { get; set; }
        #endregion

        #region [Other]
        [LoadOrUpdateOption(nameof(CustomerDetailResponse.Files))]
        public bool File { get; set; }
        #endregion
    }
}
using System.ComponentModel.DataAnnotations;

namespace Enrich.Common.Enums
{
    public class SubscriptionEnum
    {
        public enum Type
        {
            license,
            addon,
            other,
            giftcard,
        }

        public enum Period
        {
            MONTHLY,
            ONETIME,
        }

        /// <summary>
        /// Store services stage
        /// </summary>
        public enum Status
        {
            Reset = -3, // don't use
            Removed = -2, // removed affter recurring subscription
            Waiting = -1, // bought and active in future
            Deactive = 0, // has been deactive by user
            Active = 1, // has been activation
        }

        public enum LicenseStatus
        {
            [Display(Name = "InActive")]  InActive = 0, // Store_Services.active != 1 
            [Display(Name = "Expired")] Expired = 50,// Store_Services.active = 1 
            [Display(Name = "Active")] Active = 100,// Store_Services.active = 1 
            [Display(Name = "ActiveInfuture")] FutureActive = 200 // Store_Services.active = 1 
        }

        public enum TerminalStatus
        {
            [Display(Name = "InActive")] InActive = 0, // Store_Services.active != 1 
            [Display(Name = "Active")] Active = 100,// Store_Services.active = 1 
        }
        public enum Action
        {
            Reset,
            Removed,
            Waiting,
            DeActive,
            Active,
            Recurring,
        }
        public enum GroupName
        {
            [Display(Name = "Base Services")] BaseService,
            [Display(Name = "MANGO POS FEATURES")] Feature,
        }

        public enum BaseServiceCode
        {
            SMS,
            EMAIL,
            CHECKIN,
            SALONCENTER
        }

        public enum RecurringInterval
        {
            [Display(Name = "week")] Weekly,
            [Display(Name = "month")] Monthly,
            [Display(Name = "year")] Yearly,
        }
    }
}

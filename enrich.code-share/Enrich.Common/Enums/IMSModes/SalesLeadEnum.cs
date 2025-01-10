using System.ComponentModel.DataAnnotations;

namespace Enrich.Common.Enums
{
    public class SalesLeadEnum
    {
        public sealed class SpecialCode
        {
            public const string Customer = "A";
            public const string Store = "S";
        }
        public enum Type
        {
            [Display(Name = "DATA")] ImportData = 1,
            [Display(Name = "CREATE")] CreateBySaler = 2,
            [Display(Name = "REGISTER_TRIAL_ACCOUNT")] TrialAccount = 3,
            [Display(Name = "REGISTER_SLICE_ACCOUNT")] SliceAccount = 4,
            [Display(Name = "REGISTER_ON_MANGO")] SubscribeMango = 5,
            [Display(Name = "REGISTER_ON_IMS")] RegisterOnIMS = 6,
        }

        public enum Status
        {
            [Display(Name = "Lead")] Lead = 1,
            [Display(Name = "Trial Account")] TrialAccount = 2,
            [Display(Name = "Merchant")] Merchant = 3,
            [Display(Name = "Slice Account")] SliceAccount = 4,
        }
        public enum EventType
        {
            Note,
            Log
        }
    }
}
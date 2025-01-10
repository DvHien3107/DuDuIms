using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Enrich.Common.Enums
{
    public class PaymentEnum
    {
        public enum Status
        {
            [Display(Name = "Approved")] Approved,
            [Display(Name = "Success")] Success,
            [Display(Name = "Declined")] Declined,
            [Display(Name = "Failed")] Failed,
        }

        public enum Message
        {
            [Display(Name = "Free", Description = "Auto success with total payment equal 0$")]
            Free = 0,

            [Display(Name = "Credit Card", Description = "IMS payment gateway")]
            CreditCard,

            [Display(Name = "Credit Card", Description = "Recurring by credit card")]
            R_CreditCard,

            [Display(Name = "ACH", Description = "Payment by ACH")]
            ACH,

            [Display(Name = "ACH", Description = "Recurring by ACH")]
            R_ACH,
        }
    }
}
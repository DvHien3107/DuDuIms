using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Enrich.Common.Enums
{
    public class OrderEnum
    {
        public enum Status
        {
            [Display(Name = "Canceled")] Canceled,
            [Display(Name = "Open")] Open,
            [Display(Name = "Payment Later")] PaymentLater,
            [Display(Name = "Paid & Wait")] Paid_Wait, // successs
            [Display(Name = "Closed")] Closed  // successs
        }

        public enum DEPLOYMENT_PACKAGE_STATUS
        {
            /// <summary>
            /// chuan bi
            /// </summary>
            Preparation,
            /// <summary>
            /// Don hang san sang de dong goi
            /// </summary>
            Ready,
            /// <summary>
            /// dong goi/giao hang hoan tat. complete/shipped
            /// </summary>
            Complete
        }

        public enum PriceType
        {
            Trial,
            Promotional,
            Real,
        }
    }
}
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Enrich.Common.Enums
{
    public class TicketEnum
    {
        public enum TypeName
        {
            [Display(Name = "Sales")] Sales,
            [Display(Name = "Finance")] Finance,
            [Display(Name = "Onboarding")] Onboarding,
            [Display(Name = "Deployment")] Deployment,
            [Display(Name = "Support")] Support,
            [Display(Name = "Quick Support")] SupportQuick,
            [Display(Name = "IMS On Boarding")] IMSOnBoarding
        }

        public enum Status
        {
            open,
            closed
        }
    }
}
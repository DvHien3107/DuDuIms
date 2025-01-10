using System.ComponentModel.DataAnnotations;

namespace Enrich.Common.Enums
{
    public class BaseServiceEnum
    {
        public enum Code
        {
            [Display(Name = "SMS")] SMS = 1,
            [Display(Name = "Emails")] EMAIL = 2,
            [Display(Name = "Check In tablet")] CHECKIN = 3,
            [Display(Name = "Salon Center")] SALONCENTER = 4,
        }
    }
}
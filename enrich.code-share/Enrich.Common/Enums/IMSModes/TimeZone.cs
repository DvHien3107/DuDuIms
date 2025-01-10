using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Common.Enums
{
    public enum TIMEZONE
    {
        [Display(Name = "-05:00", Description = "Eastern Standard Time")] Eastern = -5,
        [Display(Name = "-06:00", Description = "Central Standard Time")] Central = -6,
        [Display(Name = "-07:00", Description = "Mountain Standard Time")] Mountain = -7,
        [Display(Name = "-08:00", Description = "Pacific Standard Time")] Pacific = -8,
        [Display(Name = "+07:00", Description = "SE Asia Standard Time")] VietNam = 7,
    }

    public enum TimeUnit
    {
        Day,
        Week,
        Month,
        Year,
        Hour,
        Minute,
        Second,
        Millisecond,
        Microsecond
    }
}

using Inner.Libs.Helpful;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Utils.IEnums
{
    public enum MemberAvailabelTimeType
    {
        [EnumAttr("Recurring")] Recurring,
        [EnumAttr("Specific")] Specific,
       
    }
}
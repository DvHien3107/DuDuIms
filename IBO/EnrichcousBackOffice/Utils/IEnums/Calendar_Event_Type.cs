using Inner.Libs.Helpful;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Utils.IEnums
{
    public enum Calendar_Event_Type
    {
        [EnumAttr("Log")] Log,
        [EnumAttr("Event")] Event,
        [EnumAttr("Note")] Note,
        [EnumAttr("DemoScheduler")] DemoScheduler,
    }
}
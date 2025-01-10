using Inner.Libs.Helpful;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Utils.IEnums
{
    public enum CommSetting
    {
        [EnumAttr("Employee")] Employee,
        [EnumAttr("Distributor")] Distributor,
    }
    public enum LevelCommSetting
    {
        [EnumAttr(1, "Member")] Member,
        [EnumAttr(2, "Leader")] Leader,
        [EnumAttr(3, "Manager")] Manager,
        [EnumAttr(4, "Director")] Director,
    }
}
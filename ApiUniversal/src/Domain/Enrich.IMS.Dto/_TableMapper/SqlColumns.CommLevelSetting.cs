using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto
{
    public sealed partial class SqlColumns
    {
        public sealed class CommLevelSetting
        {
            public const string CommPercentDirectly = "CommPercent_Directly";
            public const string CommPercentManagementOffice = "CommPercent_ManagementOffice";
            public const string CommPercentOverride = "CommPercent_Override";
        }
    }
}

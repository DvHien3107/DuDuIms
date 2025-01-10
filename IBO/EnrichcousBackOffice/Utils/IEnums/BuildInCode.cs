using Inner.Libs.Helpful;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Utils.IEnums
{
    public enum  BuildInCodeProject
    {
        [EnumAttr("Development_Ticket")] Development_Ticket,
        [EnumAttr("Support_Ticket")] Support_Ticket,
        [EnumAttr("Deployment_Ticket")] Deployment_Ticket,
    }
    public enum BuildInCodeType
    {
        [EnumAttr("Onboarding_Type")] Onboarding_Type,
        [EnumAttr("Deployment_Type")] Deployment_Type,
    }
}
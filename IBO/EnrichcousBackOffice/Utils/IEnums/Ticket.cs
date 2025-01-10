using Inner.Libs.Helpful;

namespace EnrichcousBackOffice.Utils.IEnums
{
    public enum Visible
    {
        [EnumAttr("publish")] PUBLISH,
        [EnumAttr("private")] PRIVATE,
    }

    public enum TicketStatus
    {
        [EnumAttr(21, "open")] SUPPORT_OPEN,
    }
    public enum TicketType
    {
        [EnumAttr(5, "Support")] SUPPORT,
    }
    public enum TicketIdentityType
    {
        [EnumAttr("NewSetup")] NewSetup,
        [EnumAttr("Deployment")] Deployment,
        [EnumAttr("Finance")] Finance,
        [EnumAttr("NewSalon")] NewSalon,
        [EnumAttr("PriorityOnboarding")] PriorityOnboarding,
    }
    public enum TicketDeadlineLevel
    {
         DeadlineNearly = 1,
         DeadlineOpen = 2,
         DeadlineExpired = -1,
    }
}
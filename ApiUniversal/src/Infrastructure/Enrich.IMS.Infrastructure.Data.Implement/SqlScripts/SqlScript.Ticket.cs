using DictionaryString = System.Collections.Generic.Dictionary<string, string>;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    internal static partial class SqlScript
    {
        public static class Ticket
        {
            public const string QuerySearch = @"#SELECT#
FROM T_SupportTicket WITH (NOLOCK)
#EXTENDJOIN#
WHERE 1=1 
	#CONDITION#
#ORDERBY#";
            public static DictionaryString JoinsSearch = new DictionaryString
            {
                ["TicketFeedback"] = "LEFT JOIN T_TicketFeedback TicketFeedback WITH (NOLOCK) ON T_SupportTicket.Id = TicketFeedback.TicketId"
            };


        }
    }
}

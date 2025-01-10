using Enrich.Dto;

using System.Collections.Generic;
using System.Linq;
using DictionaryString = System.Collections.Generic.Dictionary<string, string>;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    internal static partial class SqlScript 
    {
        public static class Parameters
        {
            public const string Id = "@Id";
            public const string Email = "@Email";
            public const string CustomerCode = "@CustomerCode";
            public const string OrderCode = "@OrderCode";
            public const string MxMerchantId = "@MxMerchantId";
            public const string TransactionId = "@TransactionId";
            public const string FromDate = "@FromDate";
            public const string SiteId = "@SiteId";
            public const string Year = "@Year";
        }

        public const string NoLock = "WITH (NOLOCK)";

        private static readonly DictionaryString TemplateFields = new DictionaryString
        {
            ["Ticket_HaveComments"] = "(SELECT TOP 1 1 FROM dbo.T_TicketFeedback tf WITH (NOLOCK) WHERE tf.TicketId = cteTicket.Id)",

        };
        public static void PopulateTemplateFields(List<SqlMapDto> fields)
        {
            foreach (var field in fields.Where(a => a.IsTemplateField))
            {
                field.SqlName = TemplateFields.TryGetValue(field.SqlName, out var value) ? value : string.Empty;
            }
        }

        #region Functions

        public static IEnumerable<string> GetJoins(DictionaryString joinSources, IEnumerable<string> joinKeys)
        {
            var query = from key in joinKeys.Distinct()
                        join joins in joinSources on key.ToUpper() equals joins.Key.ToUpper()
                        select joins.Value;

            return query.ToList();
        }
   
        #endregion

    }
}

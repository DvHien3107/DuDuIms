using Enrich.IMS.Dto;
using DictionaryString = System.Collections.Generic.Dictionary<string, string>;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    internal static partial class SqlScript
    {
        public static class SalesLeadComment
        {
            private const string Alias = SqlTables.SalesLeadComment;
            private const string AliasMember = SqlTables.Member;

            public static DictionaryString JoinsSearch = new DictionaryString
            {
                ["Member"] = $"LEFT JOIN {AliasMember} Member WITH (NOLOCK) ON {Alias}.CreateBy = Member.MemberNumber"
            };

            public const string GetByCustomerCode = $"SELECT cmt.* FROM {Alias} AS cmt WITH (NOLOCK) JOIN {SqlTables.SalesLead} AS sl ON sl.Id = cmt.SalesLeadId WHERE sl.CustomerCode = {Parameters.CustomerCode}";
        }
    }
}

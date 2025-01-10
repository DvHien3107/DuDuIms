using Enrich.Common.Enums;
using Enrich.IMS.Dto;
using DictionaryString = System.Collections.Generic.Dictionary<string, string>;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    internal static partial class SqlScript
    {
        public static class NewCustomerGoal
        {
            private const string Alias = SqlTables.NewCustomerGoal;

            public static string IsExistGoalForTime(int year, int month, int siteId) =>
                $"SELECT CASE WHEN COUNT(1) > 0 THEN 1 ELSE 0 END FROM NewCustomerGoal WHERE [Month] = {month} AND [Year] = {year} AND [SiteId] = {siteId}";

            /// <summary>
            /// SQL return Remaining value by store code, key
            /// </summary>
            public const string GetByYear =
                $"SELECT * FROM {Alias} {NoLock} WHERE [Year] = {Parameters.Year} AND [SiteId] = {Parameters.SiteId}";

            public static class JoinKeys
            {
                /// <summary>
                /// Extend SQL join with table Partner
                /// </summary>
                public const string Partner = "PartnerJoin";
            }

            public static DictionaryString JoinsSearch = new DictionaryString
            {
                [JoinKeys.Partner] =
                @$"LEFT JOIN {SqlTables.Partner} {NoLock} ON {Alias}.SiteId = {SqlTables.Partner}.SiteId"
            };
        }
    }
}

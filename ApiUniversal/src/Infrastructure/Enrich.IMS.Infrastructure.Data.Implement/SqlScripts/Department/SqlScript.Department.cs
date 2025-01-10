using Enrich.Common.Enums;
using Enrich.IMS.Dto;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    internal static partial class SqlScript
    {
        public static class Department
        {
            private const string Alias = SqlTables.Department;

            public static class Condition
            {
                public static string SalesTeam = $"[Type] = {DepartmentEnum.Type.SALES.ToString()} AND [Active] = {(int)DepartmentEnum.Status.Active} AND [ParentDepartmentId] IS NOT NULL";
            }
            /// <summary>
            /// SQL query return list team sales
            /// </summary>
            public const string GetSalesTeam = $"SELECT [Id] AS Value, [Name]  FROM {Alias} WITH (NOLOCK) WHERE [Type] = 'SALES' AND [Active] = 1 AND [ParentDepartmentId] IS NOT NULL";
        }
    }
}

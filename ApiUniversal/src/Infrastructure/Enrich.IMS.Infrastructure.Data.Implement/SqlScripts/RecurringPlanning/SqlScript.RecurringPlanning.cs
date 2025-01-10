using Enrich.IMS.Dto;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    internal static partial class SqlScript
    {
        public static class RecurringPlanning
        {
            private const string Alias = SqlTables.RecurringPlanning;


            public static class Conditions
            {

            }

            public const string GetByCustomerCode = @$"SELECT * FROM {Alias} WITH (NOLOCK) WHERE CustomerCode = {Parameters.CustomerCode}";
        }
    }
}
using Enrich.IMS.Dto;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    internal static partial class SqlScript
    {
        public static string QuerySearch(string Alias) => @"#SELECT#
FROM " + Alias + @" WITH(NOLOCK)
#EXTENDJOIN#
WHERE 1=1 
	#CONDITION#
#ORDERBY#";

        public static string QueryCountSumariesSearch(string Alias) => $@"SELECT DISTINCT {Alias}.*
FROM {Alias} WITH(NOLOCK)
#EXTENDJOIN#
WHERE 1=1 
	#CONDITION#
#ORDERBY#";
    }
}

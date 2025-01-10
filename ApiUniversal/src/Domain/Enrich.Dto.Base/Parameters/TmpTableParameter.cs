using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Dto.Base.Parameters
{
    /// <summary>
    /// use to parse dynamic sqlquery,
    /// separate join-query to cte table to improve performance 
    /// </summary>
    public class TmpTableParameter
    {
        /// <summary>
        /// query create table.
        /// example: create table #RankTmp  ([key] int, [RankNumber] bigint);
        /// </summary>
        public string CreateTableQuery { get; set; }

        /// <summary>
        /// table tmp name.
        /// we will replace it by another name in calculate total record ()
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// select query
        /// example: #RankTmp.[RankNumber] as [RankNumber]
        /// </summary>
        public string Select { get; set; }

        /// <summary>
        /// fields in table tmp
        /// </summary>
        public List<string> SelectFiels { get; set; } = new List<string>();

        public string Join { get; set; }

        public string Condition { get; set; }

        /// <summary>
        /// drop this table
        /// </summary>
        public string Drop { get; set; }
    }
}

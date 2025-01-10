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
    public class CTEParameter
    {
        public string CTE { get; set; }

        public string Select { get; set; }

        public string Join { get; set; }

        public string Condition { get; set; }
    }
}

using Enrich.Dto.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Dto.Base.Parameters
{
    public class SqlParserParameter
    {
        public string QueryTemplate { get; set; }
        public QueryParameter QueryParam { get; set; }
        public List<string> ExtendJoins { get; set; }
        public List<string> Conditions { get; set; }
        public List<string> FilterResultByColumnConditions { get; set; }

        public List<CTEParameter> OtherCTEs { get; set; } = new List<CTEParameter>();

        public List<TmpTableParameter> TmpTables { get; set; } = new List<TmpTableParameter>();

        public string AliasFieldRowNumber { get; set; } = "__row_num";
    }
}

using Enrich.Dto.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Dto.Base.Parameters
{
    public class FindByIdParameter : BaseParameter
    {
        public object Id { get; set; }
        public IEnumerable<SqlMapDto> Fields { get; set; }
        public IEnumerable<ConditionParameter> Conditions { set; get; }
    }
}

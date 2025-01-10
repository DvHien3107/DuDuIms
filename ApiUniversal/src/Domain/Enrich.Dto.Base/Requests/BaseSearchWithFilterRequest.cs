using Enrich.Dto.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Dto.Base.Requests
{
    public class BaseSearchWithFilterRequest<TCondition, TFilterCondition> : BaseSearchRequest where TCondition : class, new()
    {
        public TCondition Condition { get; set; }

        public TFilterCondition FilterCondition { get; set; }
    }
}

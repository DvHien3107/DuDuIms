using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enrich.DataTransfer
{
    public class BaseSearchWithFilterRequest<TCondition, TFilterCondition> : BaseSearchRequest where TCondition : class, new()
    {
        public TCondition Condition { get; set; }

        public TFilterCondition FilterCondition { get; set; }
    }
}
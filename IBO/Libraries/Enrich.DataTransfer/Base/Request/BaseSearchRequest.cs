using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enrich.DataTransfer
{
    public class BaseSearchRequest<TCondition> : BaseSearchRequest where TCondition : class, new()
    {
        public TCondition Condition { get; set; }
    }

    public class BaseSearchRequest
    {
        public string Fields { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string OrderBy { get; set; }
        public string ViewMode { get; set; } 
        public bool IsOnlyGetTotalRecords { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.UniversalApi.Base.Request
{
    public class BaseFilterConditionRequest
    {
        public List<FieldFilterDetail> Fields { get; set; }
    }
    public class FieldFilterDetail
    {
        public string Name { get; set; }

        public string Value { get; set; }

        public string DataType { get; set; }
        public string FilterType { get; set; }
    }
}
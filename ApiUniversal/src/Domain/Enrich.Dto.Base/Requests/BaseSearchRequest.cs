using Enrich.Common.Enums;
using Enrich.Dto.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Dto.Requests
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


        public Language? DisplayLanguage { get; set; }
        public ListViewMode ViewMode { get; set; } = ListViewMode.Grid;

        public bool IsOnlyGetTotalRecords { get; set; }

        public QueryParameter SqlQueryParam { get; set; }

        public bool HasField(string dtoField)
        {
            if (SqlQueryParam != null && SqlQueryParam.Fields.Any(a => a.DtoName.Equals(dtoField)))
            {
                return true;
            }

            return !string.IsNullOrWhiteSpace(Fields) && $",{Fields},".Contains($",{dtoField},");
        }


    }
}

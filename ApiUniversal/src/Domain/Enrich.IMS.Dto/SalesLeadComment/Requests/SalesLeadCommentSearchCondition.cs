using Enrich.Dto.Base;
using Enrich.IMS.Dto.Common;
using System;
using System.Collections.Generic;

namespace Enrich.IMS.Dto.SalesLeadComment
{
    public partial class SalesLeadCommentSearchCondition : BaseSearchCondition
    {
        //wating here

        public bool PopulateCountSummaries { get; set; }

        public List<KeyValueDto<string>> OnStringFields { get; set; }
    }
}

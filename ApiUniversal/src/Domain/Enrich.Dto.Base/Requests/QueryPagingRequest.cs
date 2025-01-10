using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Dto.Requests
{
    public class QueryPagingRequest : BaseSearchRequest
    {
        public string SearchText { get; set; }
        public string Where { get; set; }
    }
}

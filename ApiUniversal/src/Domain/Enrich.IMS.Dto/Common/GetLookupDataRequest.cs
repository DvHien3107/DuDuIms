using Enrich.Dto.Base;
using Enrich.IMS.Dto.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto.Common
{
    public class GetLookupDataRequest
    {
        public LookupDataType Type { get; set; }

        public NameValueIndexer<string> QueryString { get; }

        public GetLookupDataRequest(LookupDataType type, NameValueIndexer<string> queryString)
        {
            Type = type;
            QueryString = queryString ?? new NameValueIndexer<string>(queryName => string.Empty);
        }
    }
}

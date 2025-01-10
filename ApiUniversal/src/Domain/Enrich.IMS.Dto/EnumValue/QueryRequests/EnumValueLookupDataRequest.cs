using Enrich.IMS.Dto.Lookup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto.EnumValue
{
    public class EnumValueLookupDataRequest : LookupDataRequest
    {
        public string Namespace { get; set; }

        public EnumValueLookupDataRequest() : base()
        {
           
        }
    }
}

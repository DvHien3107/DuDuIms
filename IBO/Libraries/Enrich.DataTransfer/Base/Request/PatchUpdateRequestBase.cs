using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.DataTransfer.Base.Request
{
    public class PatchUpdateRequestBase
    {
        public string op { get; set; }
        public string path { get; set; }
        public object value { get; set; }

    }
}

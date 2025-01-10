using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Model.Model.Comon.Payment
{
    public class AuthorizeNetResponse
    {
        public int status { get; set; }
        public string transid { get; set; }
        public string code { get; set; }
        public string message { get; set; }
        public Object data { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Model.Model.Auth
{
    public class JWTUser
    {
        public string USERID { get; set; }
        public string PASSWORD { get; set; }
        public string STORE_NAME { get; set; }
        public string EMAILID { get; set; }
        public string PHONE { get; set; }
        public string ACCESS_LEVEL { get; set; }
        public string READ_ONLY { get; set; }
    }
}

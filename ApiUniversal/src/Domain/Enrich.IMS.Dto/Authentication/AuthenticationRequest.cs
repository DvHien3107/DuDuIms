using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto.Authentication
{
    public class AuthenticationRequest
    {      
       
        public string Email { get; set; }
        public string Password { get; set; }
        //public string AccessKey { get; set; }
        //public string SecretKey { get; set; }
    }
}

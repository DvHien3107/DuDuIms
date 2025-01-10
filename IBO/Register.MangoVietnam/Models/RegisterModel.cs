using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Register.MangoVietnam.Models
{
    public class RegisterModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FName { get; set; }
        public string LName { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime CreatedDate { get; set; }

    }
}
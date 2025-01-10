using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Register.Mango.Models
{
    public class RegisterModel
    {
        public RegisterModel()
        {
            Verify = false;
        }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FName { get; set; }
        public string LName { get; set; }
        public string PhoneNumber { get; set; }
        public InfoSalonModel InfoSalon { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ZipCode { get; set; }
        public string Timezone { get; set; }
        public bool Verify { get; set; }
        public string ErrorMessage { get; set; }

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Register.Mango.Models
{
    public partial class SubscribeModel
    {
        public string L_Email { get; set; }
        public string L_Password { get; set; }
        public string L_ContactName { get; set; }
        public string L_Phone { get; set; }
        public string L_City { get; set; }
        public string L_State { get; set; }
        public string L_NumberofSalons { get; set; }
        public string L_NumberofTechnicians { get; set; }
        public string L_Packages { get; set; }
        public string L_Timezone { get; set; }
        public string L_BusinessName { get; set; }
    }
}
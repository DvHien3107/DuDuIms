using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Promotion.Mango.Models
{
    public partial class ContactModel
    {
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string SalonName { get; set; }
        public string Message { get; set; }
    }
}
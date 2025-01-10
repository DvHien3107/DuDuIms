using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Areas.Page.Models.Customize.Ticket
{
    public class EmailAndNameModel
    {
        // [Required]
        //[DisplayName("First Name:")]
        //public string FirstName { get; set; }
        //[Required]
        //[DisplayName("Last Name:")]
        //public string LastName { get; set; }
        [Required]
        [EmailAddress]
        [DisplayName("Email Address:")]
        public string Email { get; set; }
        public bool? IsMember { get; set; }
    }
}
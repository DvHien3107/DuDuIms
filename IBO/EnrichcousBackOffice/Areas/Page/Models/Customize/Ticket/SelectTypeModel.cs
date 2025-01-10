using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EnrichcousBackOffice.Areas.Page.Models.Customize.Ticket
{
    public class SelectTypeModel
    {
        public SelectTypeModel()
        {
            TypeList = new List<SelectListItem>();
        }
        [Required]
        [DisplayName("Type:")]
        public string Type { get; set; }

        public List<SelectListItem> TypeList { get; set; }
    }
}
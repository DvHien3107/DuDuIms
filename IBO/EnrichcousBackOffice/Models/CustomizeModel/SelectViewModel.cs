using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel
{
    public class Select_view
    {

        public string name { get; set; }
        public string value { get; set; }
        public string email { get; set; }
        public bool disabled { get; internal set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel.Order
{
    public class OrderSearchByItems
    {
        public string value { get; set; }
        public string label { get; set; }
        public string name { get; set; }
        public string icon { get; set; }
        public string[] type { get; set; }
    }
}
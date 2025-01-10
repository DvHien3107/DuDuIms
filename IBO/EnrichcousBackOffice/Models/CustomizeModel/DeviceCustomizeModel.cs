using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel
{
    public class Device_Product_view
    {
        public O_Product Product { get; set; }
        public List<Device_view> item { get; set; }
    }

    public class Device_view
    {
        public O_Device Device { get; set; }
        public string Color { get; set; }
        public string VendorName { get; set; }
        public string Picture { get; set; }
        public O_Product_Model Model { get; set; }
    }
}
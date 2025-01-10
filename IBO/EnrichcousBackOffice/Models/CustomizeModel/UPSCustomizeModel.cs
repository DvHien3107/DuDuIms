using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel
{
    public class UPSCustomizeModel
    {
        
        public class Product
        {
            public string ProductCode { get; set; }
            public string Name { get; set; }
            public string Weight { get; set; }
        }

        public class Package
        {
            public string Weight { get; set; }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel
{
    public class ProductManage_view
    {
        public O_Product Product { get; set; }
        public List<O_Product_Model_CustomizeModel> Models { get; set; }
        public O_Product_Line Line { get; set; }
        public bool? IsAlreadyExistInvoice { get; set; }
    }
    public class O_Product_Model_CustomizeModel
    {
        public O_Product_Model o_Product_Model { get; set; }
        public bool? IsAlreadyExistInvoice { get; set; }
    }
}
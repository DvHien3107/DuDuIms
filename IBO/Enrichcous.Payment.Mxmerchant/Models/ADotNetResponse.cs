using System;
using System.Collections.Generic;
using System.Text;

namespace Enrichcous.Payment.Mxmerchant.Models
{
    public class ADotNetResponse
    {
        public int? status { get; set; }
        public string ResponId { get; set; }
        public string message { get; set; }
        public string JsonResult { get; set; }
        public Object Respon { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Enrichcous.Payment.Mxmerchant.Models
{
    public class CustomerProfile
    {
        public string posCustomerId { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string address { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zip { get; set; }
    }
}

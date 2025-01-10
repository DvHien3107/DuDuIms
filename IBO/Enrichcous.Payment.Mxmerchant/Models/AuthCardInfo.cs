using System;
using System.Collections.Generic;
using System.Text;

namespace Enrichcous.Payment.Mxmerchant.Models
{
    public class AuthCardInfo
    {
        //Numeric string, 13-16 digits.
        public string cardNumber { get; set; }
        //YYYY-MM
        public string expirationDate { get; set; }
        //Numeric string, 3-4 digits.
        public string cardCode { get; set; }
    }
}

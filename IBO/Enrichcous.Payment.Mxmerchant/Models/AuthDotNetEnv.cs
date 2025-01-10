using System;
using System.Collections.Generic;
using System.Text;

namespace Enrichcous.Payment.Mxmerchant.Models
{
    public class AuthDotNetEnv
    {
        public string ApiLoginID { get; set; }
        public string ApiTransactionKey { get; set; }
        public bool IsSanbox { get; set; }
    }
}

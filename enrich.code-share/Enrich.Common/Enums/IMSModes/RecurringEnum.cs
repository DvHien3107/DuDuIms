using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Common.Enums
{
    public class RecurringEnum
    {
        public enum Status
        {
            Failed = 0,
            Success = 1,
        }
        public enum PlanStatus
        {
            Disable = 0,
            Enable = 1,
        }
        public enum Type
        {
            ACH = 0,
            CreditCard = 1,
        }

        public sealed class Message
        {
            public const string CreateOrder = "Create recurring order";
            public const string PaidZero = "Order has been paid $0";
            public const string PaidWCard = "Paid with card";
            public const string PaidWACH = "Paid with ACH";
            public const string NotYet = "Not yet payment";
        }
    }
}

using System.ComponentModel.DataAnnotations;

namespace Enrich.Common.Enums
{
    public class BusinessEventEnum
    {
        public enum NameSpace
        {
            [Display(Name = "Order.Payment")] OrderPay,
        }

        public class Event
        {
            public sealed class OrderPay
            {
                public static string Later = $"{OrderEnum.Status.PaymentLater.ToString()}";
                public static string Failed = $"{PaymentEnum.Status.Failed.ToString()}";
            }
        }
        public class Description
        {
            public sealed class OrderPay
            {
                public static string Later = "Event has been created when order payment later";
                public static string Failed = "Event has been created when order payment failed";
            }
        }

        public enum Status
        {
            Waiting = 0,
            Completed = 100,
        }

        public sealed class ExtendCondition
        {
            public static string Waiting(string _event) => $"[Status] = {(int)Status.Waiting} AND [Event] = '{_event}'";
        }
    }
}
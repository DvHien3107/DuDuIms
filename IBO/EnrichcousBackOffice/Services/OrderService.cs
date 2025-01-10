using System;
using System.Linq;
using EnrichcousBackOffice.Models;

namespace EnrichcousBackOffice.Services
{
    public class OrderService : IServicesBase
    {
        internal TicketServices _ticketS = new TicketServices();
        #region Make Id, Code
        /// <summary>
        /// Order
        /// </summary>
        /// <returns></returns>
        public long MakeId()
        {
            return long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssfff"));
        }
        /// <summary>
        /// Order services
        /// </summary>
        /// <returns></returns>
        public long MakeSubscriptionId()
        {
            return long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssfff") + (new Random().Next(1, 9999) + new Random().Next(1, 100)).ToString().PadLeft(4, '0'));
        }
        /// <summary>
        /// Order device
        /// </summary>
        /// <returns></returns>
        public long MakeProductId()
        {
            return long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssfff"));
        }
        /// <summary>
        /// Order code
        /// </summary>
        /// <returns></returns>
        public string MakeOrderCode()
        {
            using (var db = new WebDataModel())
            {
                int countOfOrder = db.O_Orders.Count(o => o.CreatedAt.Value.Year == DateTime.Today.Year && o.CreatedAt.Value.Month == DateTime.Today.Month);
                return DateTime.Now.ToString("yyMM") + (countOfOrder + 1).ToString().PadLeft(4, '0') + DateTime.Now.ToString("fff");
            }
        }
        #endregion
        
    }

    public class LicenseServices : IServicesBase
    {
    }
}
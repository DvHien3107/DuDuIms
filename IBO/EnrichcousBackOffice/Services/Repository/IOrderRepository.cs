using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Models.Proc;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnrichcousBackOffice.Services.Repository
{
    public interface IOrderRepository
    {
        O_Orders GetOrderByOrderCode(string OrderCode);
        List<P_LoadOrderDetail> GetOrderDetailByOrderCode(string OrderCode);
    }
    public class OrderRepository : IOrderRepository
    {
        public O_Orders GetOrderByOrderCode(string OrderCode)
        {
            using (WebDataModel db = new WebDataModel())
            {

                var orderProduct = db.O_Orders.FirstOrDefault(o => o.OrdersCode == OrderCode);
                return orderProduct;
            }
        }

        public List<P_LoadOrderDetail> GetOrderDetailByOrderCode(string OrderCode)
        {
            using (WebDataModel db = new WebDataModel())
            {

                var orderProduct = db.Database.SqlQuery<P_LoadOrderDetail>($"exec P_LoadOrderDetail '{OrderCode}'").ToList();
                return orderProduct;
            }
        }
    }
}

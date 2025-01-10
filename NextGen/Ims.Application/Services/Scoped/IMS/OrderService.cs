using Enrich.Common.Helpers;
using Enrich.IMS.Entities;
using Pos.Application.Extensions;
using Pos.Application.Repository.IMS;
using Pos.Model.Enum.IMS;
using Pos.Model.Model.Comon;
using Pos.Model.Model.Proc.IMS;
using Pos.Model.Model.Table;
using Pos.Model.Model.Table.IMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Enrich.IMS.Dto.SqlColumns;

namespace Pos.Application.Services.Scoped.IMS
{
    public interface IOrderService
    {
        Task<O_Orders> CreateNewOrder(C_Customer customer, P_Member sale, P_Member create);
        Task<O_Orders> getOrderByCode(string orderCode);
        Task<string> addItemToOrder(O_Orders order, License_Product license_Product, C_Customer customer, 
            decimal Price, decimal Discount, int Quatity, int CountItem, bool AutoRenew);
        Task<List<Order_Subcription>> getOrderSubcriptionByCode(string orderCode);
        Task LoadOrder(string orderCode);
        Task updateOrderStauts(OrderStatus status, string OrdersCode, string PaymentMethod, string PaymentNote);
        Task<C_CustomerTransaction> getCusTransByCode(string OrderCode);
        Task<rsData> reloadOrder(string OrderCode);
    }
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IStoreServiceRepository _storeServiceRepository;
        public OrderService(IOrderRepository orderRepo, IStoreServiceRepository storeServiceRepository)
        {
            _orderRepo = orderRepo;
            _storeServiceRepository = storeServiceRepository;
        }

        public async Task<rsData> reloadOrder(string OrderCode)
        {
            rsData result = new rsData();
            var order = await getOrderByCode(OrderCode);
            if (order == null)
            {
                result.Status = 401;
                result.Message = "Order not found";
                return result;
            }
            await LoadOrder(OrderCode);
            await _storeServiceRepository.UpdateStoreService(order.CustomerCode);
            result.Status = 200;
            result.Message = "Reload Order Success";
            return result;
        }

        public async Task<C_CustomerTransaction> getCusTransByCode(string OrderCode)
        {
            return await _orderRepo.getCusTransByCode(OrderCode);
        }
        public async Task LoadOrder(string orderCode)
        {
            await _orderRepo.LoadOrder(orderCode);
        }

        private string genOrderCode(string keyType = "ImsAPI")
        {
            string dateTimeStr = DateTime.UtcNow.ToString("MMddHHmmss");
            string randomStr = StringExtension.genRandomStr(3);
            return dateTimeStr + randomStr +"-"+ keyType;
        }
        private long genItemId()
        {
            Random Rand = new Random();
            return long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssff") + Rand.Next(1, 9999).ToString().PadLeft(4, '0'));
        }
        public async Task<O_Orders> CreateNewOrder(C_Customer customer, P_Member sale, P_Member create)
        {
            O_Orders o_Orders = new O_Orders();
            o_Orders.Id = long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmss"));
            o_Orders.OrdersCode = genOrderCode();
            o_Orders.CustomerCode = customer.CustomerCode;
            o_Orders.CustomerName = customer.ContactName;
            o_Orders.SalesMemberNumber = sale.MemberNumber;
            o_Orders.SalesName = sale.FullName;
            o_Orders.GrandTotal = -1;
            o_Orders.Comment = "";
            o_Orders.CreatedAt = DateTime.UtcNow;
            o_Orders.CreatedBy = create.FullName;
            o_Orders.CreateByMemNumber = create.MemberNumber;
            o_Orders.UpdatedAt = o_Orders.CreatedAt;
            o_Orders.Status = "Open";
            o_Orders.TotalHardware_Amount = 0;
            o_Orders.ShippingFee = 0;
            await _orderRepo.InsertNewOrder(o_Orders);
            return o_Orders;
        }

        public async Task<O_Orders> getOrderByCode(string orderCode)
        {
            return await _orderRepo.getOrderByCode(orderCode);
        }
        public async Task<List<Order_Subcription>> getOrderSubcriptionByCode(string orderCode)
        {
            return await _orderRepo.getOrderSubcriptionByCode(orderCode);
        }

        public async Task<string> addItemToOrder(O_Orders order, License_Product license_Product, C_Customer customer, decimal Price, decimal Discount, int Quatity, int CountItem, bool AutoRenew)
        {
            try {
                DateTime Now = DateTime.UtcNow;
                Order_Subcription order_Subcription = new Order_Subcription();
                order_Subcription.Id = genItemId();
                order_Subcription.IsAddon = license_Product.SubscriptionDuration == "MONTHLY" ? false : true;
                order_Subcription.OrderCode = order.OrdersCode;
                order_Subcription.Product_Code = license_Product.Code;
                order_Subcription.ProductName = license_Product.Name;
                order_Subcription.Price = Price;
                order_Subcription.Discount = Discount;
                order_Subcription.CustomerCode = customer.CustomerCode;
                order_Subcription.CustomerName = customer.ContactName;
                order_Subcription.Period = license_Product.SubscriptionDuration;
                order_Subcription.PeriodRecurring = license_Product.SubscriptionDuration;
                order_Subcription.SubscriptionType = license_Product.Type;
                order_Subcription.PurcharsedDay = order.InvoiceDate;
                order_Subcription.NumberOfItem = CountItem;
                order_Subcription.SubscriptionQuantity = Quatity;
                order_Subcription.AutoRenew = AutoRenew;
                order_Subcription.StartDate = Now;
                if (order_Subcription.IsAddon ?? false)
                {
                    order_Subcription.EndDate = Now;
                }
                else
                {
                    order_Subcription.EndDate = Now.AddMonths(Quatity);
                }
                order_Subcription.Actived = true;
                await _orderRepo.InsertOrderSubcription(order_Subcription);
                return "Success";
            }
            catch(Exception e)
            {
                Console.Error.WriteLine(e.Message+"---"+ e.StackTrace);
                return e.Message;
            }
            
        } 
    
        public async Task updateOrderStauts(OrderStatus status, string OrdersCode, string PaymentMethod, string PaymentNote)
        {
            await _orderRepo.updateOrderStauts(status, OrdersCode, PaymentMethod, PaymentNote);
            await reloadOrder(OrdersCode);
        }
    }
}

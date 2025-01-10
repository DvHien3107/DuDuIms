using Pos.Application.Repository.IMS;
using Pos.Model.Model.Proc.IMS;
using Pos.Model.Model.Table;
using Pos.Model.Model.Table.IMS;
using SlackAPI.WebSocketMessages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Application.Services.Scoped.IMS
{
    public interface IRecurrringService
    {
        Task doRecurringLicense();
    }

    public class RecurrringService : IRecurrringService
    {
        private readonly IRecurringRepository _recurringRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IMemberRespository _memberRepository;
        private readonly ICustomerRespository _customerRespository;
        private readonly IOrderService _orderService;
        public RecurrringService(IOrderService orderService, IRecurringRepository recurringRepository, IOrderRepository orderRepository, IMemberRespository memberRespository, ICustomerRespository customerRespository) { 
            _recurringRepository = recurringRepository;
            _orderRepository = orderRepository;
            _memberRepository = memberRespository;
            _customerRespository = customerRespository;
            _orderService = orderService;
        }

        public async Task doRecurringLicense()
        {
            List<string> listSolvedCustomer = new List<string>();
            List<P_GetRecurringList> store_Services = await _recurringRepository.getListRecurringData();
            foreach(var item in store_Services)
            {
                List<string> checkSolvedCustomer = listSolvedCustomer.Where(x => x == item.CustomerCode).ToList();
                if(checkSolvedCustomer.Count == 0)
                {
                    List<P_GetRecurringList> listRecuringForThisCutomer = store_Services.Where(x => x.CustomerCode == item.CustomerCode).ToList();

                    listSolvedCustomer.Add(item.CustomerCode);
                    string newOrderCode =  await cloneOrderByCode(item.OrderCode);
                    decimal amountToCharge = listRecuringForThisCutomer.Sum(x => x.RecurringPrice ?? 0);


                }
            }
        }

        public async Task<string> cloneOrderByCode(string OrderCode)
        {
            O_Orders o_Orders = await _orderRepository.getOrderByCode(OrderCode);
            List<Order_Subcription> orderSub = await _orderRepository.getOrderSubcriptionByCode(OrderCode);
            P_Member p_MemberSale = await _memberRepository.getMember(o_Orders.SalesMemberNumber);
            P_Member p_MemberCreate = await _memberRepository.getMember(o_Orders.CreateByMemNumber);
            C_Customer c_Customer = await _customerRespository.getCustomer(o_Orders.CustomerCode);
            O_Orders newOrder = await _orderService.CreateNewOrder(c_Customer, p_MemberSale, p_MemberCreate);
            string listError = "";
            foreach (var recurring in orderSub)
            {
                License_Product licenseProduct = await _orderRepository.getLicenseProductByCode(recurring.Product_Code);
                //, decimal Discount, int Quatity, int CountItem, bool AutoRenew
                string resultAddItemToOrder = await _orderService.addItemToOrder(newOrder, licenseProduct, c_Customer, recurring.Price ?? 0, recurring.Discount ?? 0, 1, recurring.SubscriptionQuantity ?? 1, true);
                if (resultAddItemToOrder != "Success")
                {
                    listError += (resultAddItemToOrder + ",");
                }
            }
            await _orderService.LoadOrder(newOrder.OrdersCode);
            return newOrder.OrdersCode;
        }
    }
}

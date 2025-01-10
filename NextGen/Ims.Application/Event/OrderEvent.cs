using Enrich.IMS.Entities;
using Pos.Application.Event.POS;
using Pos.Application.Repository.IMS;
using Pos.Application.Services.Scoped;
using Pos.Application.Services.Scoped.IMS;
using Pos.Model.Model.Request;
using Pos.Model.Model.Respons;
using Pos.Model.Model.Table.IMS;
using SlackAPI.WebSocketMessages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Application.Event
{
    public interface IOrderEvent
    {
        Task<EventResponsive> CloseOrderByCode(string OrderCode);
        Task ClosePendingOrder();
        Task<EventResponsive> StoreChane(StoreLicenses modelLicense);
        Task CloseOrder(string OrderCode);
    }
    public class OrderEvent : IOrderEvent
    {
        private readonly IOrderRepository _orderService;
        private readonly ICustomerService _customerService;
        private readonly ILicenseEvent _licenseEvent;
        public OrderEvent(IOrderRepository orderService, ILicenseEvent licenseEvent, ICustomerService customerService) {
            _orderService = orderService;
            _licenseEvent = licenseEvent;
            _customerService = customerService;
        }
        public async Task ClosePendingOrder()
        {
            var lstOrder = await _orderService.getPendingLst();
            foreach (var item in lstOrder)
            {
               await CloseOrderByCode(item.OrdersCode);
               
            }
        }
        
        public async Task CloseOrder(string OrderCode)
        {
            var lstOrder = await _orderService.checkOrderToClose(OrderCode);
            if (lstOrder != null)
            {
                await CloseOrderByCode(lstOrder.OrdersCode);
            }
        }

        public async Task<EventResponsive> CloseOrderByCode(string OrderCode)
        {
            EventResponsive result = new EventResponsive();
            result.status = 400;
            result.message = "EmptyContent";
            try
            {
                List<License_Product_Item> license_Product_Items = new List<License_Product_Item>();
                List<Order_Subcription> _order_Subscription = await _orderService.getListSubcription(OrderCode);
                foreach (var item in _order_Subscription)
                {
                    List<License_Product_Item> tmp = await _orderService.getListLicenseCode(item.Product_Code);
                    license_Product_Items.AddRange(tmp);
                }
                O_Orders order = await _orderService.getOrderByCode(OrderCode);
                StoreLicenses modelLicense = await _customerService.getCustomerInfoByCode(order.CustomerCode, order.OrdersCode);
                List<Licenses> licensesLst = new List<Licenses>();
                foreach (var license in license_Product_Items)
                {
                    Licenses licenses = new Licenses();
                    licenses.licenseCode = license.License_Item_Code;
                    licenses.count_limit = license.Value ?? 0;
                    licensesLst.Add(licenses);
                }
                modelLicense.licenses = licensesLst;
                var ressult = await StoreChane(modelLicense);
                if(ressult.status == 200)
                {
                    await _orderService.CloseOrder(OrderCode);
                }
                return ressult;

            } catch (Exception ex)
            {
                result.status = 500;
                result.message = ex.Message;
                result.data = ex;
            }
           
            return result;
        }

        public async Task<EventResponsive> StoreChane(StoreLicenses modelLicense)
        {
            EventResponsive result = new EventResponsive();
            result.status = 400;
            result.message = "EmptyContent";
            try
            {
                await _licenseEvent.StoreChange(modelLicense);
                result.status = 200;
                result.message = "Change Store Success";
            }
            catch (Exception ex)
            {
                result.status = 500;
                result.message = ex.Message;
                result.data = ex;
            }

            return result;
        }
    }
}

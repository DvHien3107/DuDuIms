using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Areas.PaymentGate.ModelsView.DTO;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Utils.IEnums;
using Inner.Libs.Helpful;

namespace EnrichcousBackOffice.Areas.PaymentGate.Services
{
    public class PurchasesService
    {
        public string MakeNewOrderCode()
        {
            using (var db = new WebDataModel())
            {
                int countOfOrder = db.O_Orders.Count(o => o.CreatedAt.Value.Year == DateTime.Today.Year && o.CreatedAt.Value.Month == DateTime.Today.Month);
                return DateTime.UtcNow.ToString("yyMM") + (countOfOrder + 1).ToString().PadLeft(4, '0') + DateTime.UtcNow.ToString("fff");
            }
        }

        public string LastOrderCode(string customerCode, HttpSessionStateBase Session = null)
        {
            using (var db = new WebDataModel())
            {
                if (string.IsNullOrEmpty(customerCode))
                {
                    string authInfo = Session?[AreaPayConst.AUTH_BASIC_KEY]?.ToString();
                    if (string.IsNullOrEmpty(authInfo) == false && authInfo.StartsWith("Basic"))
                    {
                        string credentials = authInfo.Substring("Basic ".Length).Trim();
                        long customerId = long.Parse(credentials.FromBase64().Split(':')[0]);
                        var cus = db.C_Customer.Find(customerId);
                        customerCode = cus?.CustomerCode ?? "";
                    }
                }
                var open_invoice = InvoiceStatus.Open.ToString();
                var lastOrder = db.O_Orders.Where(order => order.Status == open_invoice && order.CustomerCode == customerCode && order.IsDelete != true)
                    .OrderByDescending(_order => _order.CreatedAt).FirstOrDefault();
                return lastOrder?.OrdersCode ?? "";
            }
        }
        
        public O_Orders MakeNewOrder(long customerId, InvoiceRequest invoices)
        {
            using (var db = new WebDataModel())
            {
                var effectDate = DateTime.UtcNow;
                var cus = db.C_Customer.Find(customerId);
                var ordersCode = MakeNewOrderCode();
                var orders = new O_Orders();
                if (string.IsNullOrEmpty(invoices.LastOrder) == false)
                {
                    orders = db.O_Orders.FirstOrDefault(_ord => _ord.OrdersCode == invoices.LastOrder) ?? new O_Orders();
                }
                else
                {
                    orders.Id = long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssfff"));
                    orders.OrdersCode = ordersCode;
                }
                orders.CustomerCode = cus.CustomerCode;
                orders.CustomerName = cus.BusinessName;
                orders.GrandTotal = invoices.GrandTotal;
                orders.CreatedAt = DateTime.UtcNow;
                orders.CreatedBy = "IMS System";
                orders.TotalHardware_Amount = invoices.Invoices.Select(invoice=>invoice.Price*invoice.Quantity).Sum();
                orders.ShippingFee = 0;
               // orders.Service_Amount = invoices.GrandTotal - orders.TotalHardware_Amount;
                orders.DiscountAmount = 0;
                orders.DiscountPercent = 0;
                orders.TaxRate = 0;
                orders.Comment = "";
                orders.InvoiceDate = DateTime.UtcNow.Date;
                //orders.DueDate = effectDate.AddMonths(1);
                orders.InvoiceNumber = long.Parse(orders.OrdersCode);
                orders.Status = InvoiceStatus.Open.ToString();
                orders.BundelStatus = UserContent.DEPLOYMENT_PACKAGE_STATUS.Preparation.ToString();
                return orders;
            }
        }

        public List<Order_Subcription> MakeNewSubscriptions(long customerId, InvoiceRequest invoices, string orderCode)
        {
            using (var db = new WebDataModel())
            {
                List<Order_Subcription> subscriptions = new List<Order_Subcription>();
                var today = DateTime.UtcNow;
                var cus = db.C_Customer.Find(customerId);
                invoices.Invoices.FindAll(invoice => invoice.Type == "addon" || invoice.Type == "license").ForEach(invoice =>
                {
                    License_Product addon = db.License_Product.FirstOrDefault(lp=>lp.Code==invoice.Code);
                    long newId = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssfff") + (new Random().Next(1, 9999)).ToString().PadLeft(4, '0'));
                    while (subscriptions.Any(sub => sub.Id == newId))
                    {
                        newId += 1;
                    }
                    subscriptions.Add(new Order_Subcription
                    {
                        Id = newId,
                        StoreCode = cus.StoreCode,
                        OrderCode = orderCode,
                        ProductId = addon.Id,
                        ProductName = addon.Name,
                        Product_Code = addon.Code,
                        CustomerCode = cus.CustomerCode,
                        CustomerName = cus.BusinessName,
                        IsAddon = invoice.Type == "addon",
                        Price = invoice.Price,
                        Quantity = 1,
                        NumberOfItem = db.License_Product_Item.Count(item => item.License_Product_Id == addon.Id),
                        Actived = true,
                        AutoRenew = false,
                        Period = addon.SubscriptionDuration,
                        PurcharsedDay = today,
                        StartDate = today,
                        EndDate = addon.SubscriptionDuration == "MONTHLY" ? today.AddMonths(1) : new Order_Subcription().EndDate
                    });
                });
                return subscriptions;
            }
        }

        public List<Order_Products> MakeNewDevices(long customerId, InvoiceRequest invoices, string orderCode)
        {
            using (var db = new WebDataModel())
            {
                List<Order_Products> devices = new List<Order_Products>();
                var today = DateTime.UtcNow;
                invoices.Invoices.FindAll(invoice => invoice.Type == "device").ForEach(invoice =>
                {
                    O_Product_Model item = db.O_Product_Model.FirstOrDefault(model => model.ModelCode == invoice.Code);
                    long newId = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssfff") + (new Random().Next(1, 9999)).ToString().PadLeft(4, '0'));
                    while (devices.Any(device => device.Id == newId))
                    {
                        newId += 1;
                    }
                    devices.Add(new Order_Products()
                    {
                        Id = newId,
                        OrderCode = orderCode,
                        ProductCode = item.ProductCode,
                        ProductName = item.ProductName,
                        Price = item.Price ?? 0,
                        Quantity = invoice.Quantity,
                        TotalAmount = (item.Price ?? 0) * (invoice.Quantity),
                        CreateBy = "System",
                        CreateAt = today,
                        Feature = item.Color,
                        ModelCode = item.ModelCode,
                        ModelName = item.ModelName,
                        BundleId = null,
                        BundleQTY = null,
                    });
                });
                return devices;
            }
        }
    }
}
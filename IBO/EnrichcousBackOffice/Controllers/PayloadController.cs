using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Models.CustomizeModel;

namespace EnrichcousBackOffice.Controllers
{
    public class PayloadController : Controller
    {
        // GET: Payload
        private long OrderId = 0;
        public ActionResult Index()
        {
            string encrypt = Request["code"];
            if (!string.IsNullOrEmpty(encrypt))
            {
                string decrypt = AppLB.Encryption.AES.Decrypt(encrypt);
                OrderId = long.Parse(decrypt.Split('|')[0]);
                string MerchantCode = decrypt.Split('|')[1];
                string UrlBack = decrypt.Split('|')[2];

                var db = new WebDataModel();
                ViewBag.Code = encrypt;
                var order = db.O_Orders.Find(OrderId);
                ViewBag.Order = order;
                ViewBag.OrderProducts = db.Order_Products.Where(op=>op.OrderCode== order.OrdersCode).ToList();
                ViewBag.OrderService = db.Order_Subcription.Where(os => os.OrderCode == order.OrdersCode).ToList();
                ViewBag.Merchant = db.C_Customer.Where(c => c.CustomerCode == MerchantCode).FirstOrDefault();
            }
            return View();
        }
        public JsonResult SendPayment()
        {
            try
            {
                var db = new WebDataModel();
                try
                {
                    string encrypt = Request["code"];
                    string decrypt = AppLB.Encryption.AES.Decrypt(encrypt);
                    OrderId = long.Parse(decrypt.Split('|')[0]);
                }
                catch
                {
                    throw new Exception("Authentication failed!");
                }
                var order = db.O_Orders.Find(OrderId);
                if (order == null)
                {
                    throw new Exception("Order not found!");
                }
                var paymentInfo = new PaymentInfo
                {
                    OrderId = order.OrdersCode,
                    Amount = (long)order.GrandTotal,
                    CardType = Request["card_type"],
                    CardNumber = Request["card_number"],
                    CardExpiry = Request["card_expiry"],
                    CardHolderName = Request["card_holder_name"],
                    Cvv = Request["cvv"],
                    Email = Request["email"],
                    Phone = Request["phone"]
                };

                AppLB.NuveiLB.SendXML.Nuvei_payment(paymentInfo);
                return Json(new object[] { true, "" });
            }
            catch(Exception e)
            {
                return Json(new object[] { true, "Fail: " + e.Message });
            }
        }
    }
}
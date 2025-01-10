using System;
using System.Linq;
using System.Web.Mvc;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.AppLB;
using System.Threading.Tasks;
using Newtonsoft.Json;
using EnrichcousBackOffice.ViewControler;
using Enrichcous.Payment.Mxmerchant.Models;
//using static EnrichcousBackOffice.Models.CustomizeModel.SendGridEmailTemplateData;
using EnrichcousBackOffice.Areas.PaymentGate.Services;
using Enrich.IServices;
using Enrich.IServices.Utils.Mailing;
using System.Xml;
using System.Collections.Generic;
using EnrichcousBackOffice.Services;
using Enrich.IServices.Utils.Universal;

namespace EnrichcousBackOffice.Controllers
{
    [AllowAnonymous]
    public class TestController : Controller
    {
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly ILogService _logService;
        private readonly IMailingService _mailingService;
        private readonly IEnrichUniversalService _enrichUniversalService;

        public TestController(IEmailTemplateService emailTemplateService, ILogService logService, IMailingService mailingService, IEnrichUniversalService enrichUniversalService)
        {
            _emailTemplateService = emailTemplateService;
            _logService = logService;
            _mailingService = mailingService;
            _enrichUniversalService = enrichUniversalService;
        }

        // GET: Test
        public async Task<ActionResult> Index()
        {            
            return View();
        }

        public JsonResult trackkingContext()
        {
            var context = _enrichUniversalService.readContext();
            return Json(context , JsonRequestBehavior.AllowGet);
        }

        #region demo Send Email After Delivery
        public async Task<ActionResult> SendEmailAfterDelivery(string trackingNumber = "1Z76X38E0290480105")
        {
            try
            {
                WebDataModel db = new WebDataModel();
                var UPSTracking = db.Order_UPSTracking.Where(x => x.TrackingNumber == trackingNumber).FirstOrDefault();
                if (UPSTracking == null)
                {
                    throw new Exception("UPS tracking not found");
                }

                var order = db.O_Orders.Where(o => o.OrdersCode == UPSTracking.OrderCode).FirstOrDefault();
                if (order == null)
                {
                    throw new Exception("Customer not found");
                }

                var customer = db.C_Customer.Where(c => c.CustomerCode == order.CustomerCode).FirstOrDefault();
                if (customer == null)
                {
                    throw new Exception("Customer not found");
                }

                var customerAddress = customer?.BusinessAddressStreet + ", " + customer?.BusinessCity + ", " + customer?.BusinessState + ", " + customer?.BusinessZipCode + ", " + customer.BusinessCountry;
                var packageCount = db.Order_UPSPackage.Where(x => x.OrderUPSTrackingId == UPSTracking.TrackingNumber).Count();

                var _bodyData = "<tbody>";
                foreach (var item in db.Order_Products.Where(p => p.OrderCode == order.OrdersCode).ToList())
                {
                    _bodyData += "<tr><td>" + item.ModelCode + "</td><td>" + item.ProductName + "</td><td style='text-align:center'>" + item.Quantity + "</td></tr>";
                }
                _bodyData += "</tbody>";

                var _subject = "[ENRICHCOUS]Product shipment and tracking info";
                var _body = "<b><i style='font-family:Georgia,Times,serif; font-size:20px'>Dear " + customer?.OwnerName + ",</i></b><br/>" +
                    "Thanks so much for choosing Enrich. We have shipped out your order products.<br/><br/>" +
                    "<table style='border-collapse:collapse; width:100%'><tbody>" +
                    "<tr style='border-bottom:1px solid grey'><td colspan='2' style='text-align:right'>ORDER <b>#" + order?.OrdersCode + "</b></td></tr>" +
                    "<tr><td><br><b>Customer</b><br>" + customer?.OwnerName + "<br>" + customer?.OwnerMobile + "<br>" + (!string.IsNullOrEmpty(customerAddress) ? customerAddress : "") + "</td>" +
                    "<td><br><b>Shipping Address</b ><br>" + customer?.OwnerName + "<br>" + customer?.OwnerMobile + "<br>" + order?.ShippingAddress?.Replace("|", ",") + "</td></tr>" +
                    "<tr><td colspan='2'></td></tr>" +
                    "<tr><td colspan='2'><br/><b>Shipping unit:</b> UPS</td></tr>" +
                    "<tr><td colspan='2'></td></tr>" +
                    "<tr><td colspan='2'><br/>" +
                    "<table class='table_border' style='width:100%;border-collapse:collapse;border:1px solid grey;' cellpadding='5'>" +
                    "<thead style='background-color:#f1eeee'><tr style='font-weight:bold;text-align:center'>" +
                    "<td>Model#</td><td>Hardware Name</td><td>Quantity</td>" +
                    "</tr></thead>" + _bodyData +
                    "</table>" +
                    "</td></tr>" +
                    "</tbody></table><br/>" +
                    "The tracking number is <b>" + UPSTracking?.TrackingNumber + "</b><br/>Number of package(s): <b>" + packageCount + "</b><br/><br/>" +
                    "You can track via this link:<br/>" +
                    "<a href='https://www.ups.com/track?loc=en_US&requester=ST/' taget='_blank'>https://www.ups.com/track?loc=en_US&requester=ST/</a><br/><br/>" +
                    "Thanks again for your great support!<br/>Enrich Inventory Team";

                var result = await _mailingService.SendBySendGrid("locdv95@gmail.com", "Loc", _subject, _body);
                return Redirect("/home");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

    }
}
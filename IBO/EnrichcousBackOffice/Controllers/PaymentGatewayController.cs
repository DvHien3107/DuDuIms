using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.ViewControler;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EnrichcousBackOffice.Controllers
{
    [Authorize]
    public class PaymentGatewayController : Controller
    {
        // GET: PaymentGateway
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> Payment(long? id)
        {
            WebDataModel db = new WebDataModel();
            //using (var TranS = db.Database.BeginTransaction())
            //{
                try
                {
                    var _order = db.O_Orders.Find(id);
                    if (_order != null)
                    {
                       await InventoryViewService.ConvertToInvoice(_order, db,true, User.Identity.Name);
                       await OrderViewService.CheckMerchantWordDetermine(_order.CustomerCode);

                    //TranS.Commit();
                    //TranS.Dispose();
                    TempData["s"] = "Convert to invoice success.";

                        return Redirect("/order/index");
                    }
                    else
                    {
                        throw new Exception("Order does not exist.");
                    }
                }
                catch (Exception ex)
                {
                    //TranS.Dispose();
                    TempData["e"] = ex.Message;
                    return Redirect("/order/estimates");
                }
            //}
        }
    }
}
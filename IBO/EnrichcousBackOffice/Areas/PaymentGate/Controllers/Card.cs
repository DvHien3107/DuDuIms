using System;
using System.Linq;
using System.Web.Http;
using System.Web.Mvc;
using EnrichcousBackOffice.Areas.PaymentGate.ModelsView.DTO;
using EnrichcousBackOffice.Areas.PaymentGate.Services;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Utils.AppConfig;
using Inner.Libs.Helpful;

namespace EnrichcousBackOffice.Areas.PaymentGate.Controllers
{
    public class CardController : Base
    {
        internal CustomerService _customer = new CustomerService();
        internal PaymentService _payment = new PaymentService();
        // GET
        public ActionResult Index(string key)
        {
            if (!string.IsNullOrEmpty(key))
            {
                CustomerService.StoreSessionLogin(AuthParse(key));
            }
            try
            {
                if ((Session[PURCHASES_AGENT]?.ToString() ?? "") != "")
                {
                    ViewBag.AgentAction = true;
                    ViewBag.Error = "You do not have permission!";
                    throw new Exception();
                }
                ViewBag.Error = SessionMsg(CARD_ERROR);
                using (var db = new WebDataModel())
                {
                    var Auth = (Session[AreaPayConst.AUTH_BASIC_KEY]?.ToString() ?? "") != "";
                    ViewBag.Auth = Auth;
                    if (Auth)
                    {
                        long cus_id = CustomerIdAuth();
                        var Customer = db.C_Customer.Find(cus_id);
                        var datenow = DateTime.UtcNow.Date;
                        ViewBag.credit = db.C_CustomerCard.AsEnumerable().Where(card => card.CustomerCode == Customer.CustomerCode
                        && card.Active == true).ToList();

                        ViewBag.expired_credit = db.C_CustomerCard.AsEnumerable().Where(card => card.CustomerCode == Customer.CustomerCode
                        && card.Active == true).ToList();

                        ViewBag.deactive_credit = db.C_CustomerCard.AsEnumerable().Where(card => card.CustomerCode == Customer.CustomerCode
                        && card.Active != true).ToList();

                        ViewBag.Transaction = (
                            from t in db.C_CustomerTransaction.AsEnumerable()
                            where t.CustomerCode == Customer.CustomerCode
                            join o in db.O_Orders on t.OrdersCode equals o.OrdersCode
                            select new Transaction_view
                            {
                                Card_id = t.Card,
                                createAt = t.CreateAt,
                                order = t.OrdersCode,
                                order_pending = (o.Status == "Open"),
                                invoice_date = o.CreatedAt,
                                //type = (o.TotalHardware_Amount > 0 ? "Hardware" : "") + (o.TotalHardware_Amount > 0 && o.Service_Amount > 0 ? " & " : "") + (o.Service_Amount > 0 ? "Service" : ""),
                                amount = t.Amount,
                                status = t.PaymentStatus,
                                noty = t.MxMerchant_authMessage
                            }
                        ).ToList();
                        ViewBag.CompanyInfo = db.SystemConfigurations.FirstOrDefault();
                        ViewBag.country = db.Ad_Country.ToList();
                        ViewBag.states = db.Ad_USAState.ToList();
                        ViewBag.cus = db.C_Customer.FirstOrDefault(c => c.CustomerCode == Customer.CustomerCode);
                        if ((Request.Headers["Referer"]?.ToLower() ?? "").Contains("paymentgate/purchases"))
                        {
                            ViewBag.HadHistory = (Request.Headers["Referer"]?.ToLower() ?? "").Contains("paymentgate/purchases");
                            ViewBag.History = Request.Headers["Referer"];
                        }
                        return View(Customer);
                    }
                }
                return View();
            }
            catch (Exception e)
            {
                return View();
            }
        }




        public ActionResult Login(string email, string password)
        {
            try
            {
                _customer.Login(email, password, Session);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                MakeErrorResponse(e, CARD_ERROR);
            }
            return RedirectToAction("Index");
        }

        public ActionResult AddNewCard([FromBody] TransRequest info)
        {
            using (var db = new WebDataModel())
            {
                var cus = CustomerAuth();
                if (cus == null)
                {
                    BasicAuthClear();
                    goto REDIRECT;
                }
                try
                {
                    info.StoreCode = cus.StoreCode;
                    info.MxMerchant_Id = long.Parse(cus.MxMerchant_Id??"0");
                    _payment.NewCard(info);
                    TempData["s"] = "Add new card completed!";
                }
                catch (Exception ex)
                {
                    TempData["e"] = ex.Message;
                    MakeErrorResponse(ex, CARD_ERROR);
                }

                return Redirect("index");
            }
        REDIRECT:
            return RedirectToAction("Index", new { key = info.Key });
        }

        public JsonResult GetCardInfo(string CardId)
        {
            try
            {
                using (var db = new WebDataModel())
                {
                    var cus = CustomerAuth();
                    var card = db.C_CustomerCard.Find(CardId);

                    if (cus == null)
                    {
                        throw new Exception("authentication failed");
                    }
                    if (cus?.CustomerCode != card?.CustomerCode)
                    {
                        throw new Exception("Card not found!");
                    }
                    return Json(new object[] { true, card, true });
                }
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });
            }
        }

        //public JsonResult SaveEditCard(C_CustomerCard card)
        //{
        //    try
        //    {
        //        using (var db = new WebDataModel())
        //        {
        //            var cus = CustomerAuth();
        //            if (cus == null)
        //            {
        //                throw new Exception("authentication failed");
        //            }
        //            var EditCard = db.C_CustomerCard.Find(card.Id);
        //            if (cus?.CustomerCode != EditCard?.CustomerCode)
        //            {
        //                throw new Exception("Card number is not match!");
        //            }
        //            card.StoreCode = cus.StoreCode;
        //            card.CustomerCode = cus.CustomerCode;
        //            card.MxMerchant_Id = EditCard.MxMerchant_Id;
        //            card.MxMerchant_Token = EditCard.MxMerchant_Token;
        //            _payment.EditCard(card);
        //            return Json(new object[] { true, EditCard.Active, Check_expiry(EditCard.CardExpiry) });
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        return Json(new object[] { false, e.Message });
        //    }
        //}

        public JsonResult ChangeStatusCard(string id, bool active)
        {
            try
            {
                using (var db = new WebDataModel())
                {
                    var cus = CustomerAuth();
                    var card = db.C_CustomerCard.Find(id);

                    if (cus == null)
                    {
                        throw new Exception("authentication failed");
                    }
                    if (cus?.CustomerCode != card?.CustomerCode)
                    {
                        throw new Exception("Card not found!");
                    }
                    if (card.IsDefault && !active)
                    {
                        throw new Exception("Can't deactivate Default card!");
                    }
                    card.Active = active;
                    db.Entry(card).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    return Json(new object[] { true, active, true });
                }
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });
            }

        }
        public ActionResult gotoPayment(string OrdersCode)
        {
            using (var db = new WebDataModel())
            {
                long cus_id = CustomerIdAuth();
                var Customer = db.C_Customer.Find(cus_id);
                return Redirect($"{AppConfig.Host}/PaymentGate/Pay/?key={OrdersCode.ToBase64()}:{Customer.MD5PassWord.ToBase64()}");
            }
        }
    }


}
using DataTables.AspNet.Core;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Areas.PaymentGate.ModelsView.DTO;
using EnrichcousBackOffice.Areas.PaymentGate.Services;
using EnrichcousBackOffice.Models;
using Inner.Libs.Helpful;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EnrichcousBackOffice.Controllers
{
    public class PartnerController : UploadController
    {
        private WebDataModel db = new WebDataModel();
        private Dictionary<string, bool> access = AppLB.Authority.GetAccessAuthority();
        private P_Member cMem = AppLB.Authority.GetCurrentMember();
        internal PaymentService _payment = new PaymentService();
        // GET: Partner
        public ActionResult Index()
        {
            if (access.Any(k => k.Key.Equals("partner_manage")) == false || access["partner_manage"] != true)
            {
                return Redirect("/home/forbidden");
            }
            if (!string.IsNullOrEmpty(cMem.BelongToPartner) && access.Any(k => k.Key.Equals("partner_profile")) == true && access["partner_profile"] == true)
            {
                var partner = db.C_Partner.AsEnumerable().FirstOrDefault(c => c.Code == cMem.BelongToPartner.Split(',').First()) ?? new C_Partner { };
                return RedirectToAction("/save", new { Id = partner.Id });
            }
            ViewBag.SaleMembers = db.P_Member.Where(m => m.Active == true && db.P_Department.Where(d => d.Type == "SALES").Select(d => d.Id.ToString()).Any(n => m.DepartmentId.Contains(n))).ToList();
            return View();
        }

        public ActionResult LoadListPartner(IDataTablesRequest dataTablesRequest, string SearchText, string SalePerson, int Status = -1)
        {
            var list_partner = db.C_Partner.Where(c => c.Status > -1 && ((cMem.BelongToPartner ?? "").Contains(c.Code) || string.IsNullOrEmpty(cMem.BelongToPartner))).AsEnumerable();
            if (Status > -1)
            {
                list_partner = list_partner.Where(c => c.Status == Status);
            }
            if (!string.IsNullOrEmpty(SalePerson))
            {
                if (SalePerson == "Unassigned") list_partner = list_partner.Where(c => !db.P_Member.Any(p => p.BelongToPartner.Contains(c.Code)));
                else list_partner = list_partner.Where(c => db.P_Member.Where(p => p.MemberNumber == SalePerson).Any(p => p.BelongToPartner.Contains(c.Code)));
            }
            if (!string.IsNullOrEmpty(SearchText))
            {
                list_partner = list_partner.Where(c => CommonFunc.SearchName(c.Address, SearchText) ||
                                                    CommonFunc.SearchName(c.Code, SearchText) ||
                                                    CommonFunc.SearchName(c.ContactName, SearchText) ||
                                                    CommonFunc.SearchName(c.Description, SearchText) ||
                                                    CommonFunc.SearchName(c.Email, SearchText) ||
                                                    CommonFunc.SearchName(c.Hotline, SearchText) ||
                                                    CommonFunc.SearchName(c.Phone, SearchText) ||
                                                    CommonFunc.SearchName(c.Name, SearchText));
            }

            var colSort = dataTablesRequest.Columns.Where(c => c.Sort != null).FirstOrDefault();
            switch (colSort.Name)
            {
                case "Code":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        list_partner = list_partner.OrderBy(s => s.Code);
                    }
                    else
                    {
                        list_partner = list_partner.OrderByDescending(s => s.Code);
                    }
                    break;
                case "Name":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        list_partner = list_partner.OrderBy(s => s.Name);
                    }
                    else
                    {
                        list_partner = list_partner.OrderByDescending(s => s.Name);
                    }
                    break;
                case "ContactName":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        list_partner = list_partner.OrderBy(s => s.ContactName);
                    }
                    else
                    {
                        list_partner = list_partner.OrderByDescending(s => s.ContactName);
                    }
                    break;
                case "Status":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        list_partner = list_partner.OrderBy(s => s.Status);
                    }
                    else
                    {
                        list_partner = list_partner.OrderByDescending(s => s.Status);
                    }
                    break;
                case "Description":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        list_partner = list_partner.OrderBy(s => s.Description);
                    }
                    else
                    {
                        list_partner = list_partner.OrderByDescending(s => s.Description);
                    }
                    break;
                case "LastUpdated":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        list_partner = list_partner.OrderBy(s => s.UpdateAt);
                    }
                    else
                    {
                        list_partner = list_partner.OrderByDescending(s => s.UpdateAt);
                    }
                    break;
                default:
                    list_partner = list_partner.OrderByDescending(s => s.Code);
                    break;
            }

            var totalRecord = list_partner.Count();
            list_partner = list_partner.Skip(dataTablesRequest.Start).Take(dataTablesRequest.Length);
            var dataView = list_partner.ToList();
            return Json(new
            {
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord,
                draw = dataTablesRequest.Draw,
                data = dataView
            });
        }

        public JsonResult LoadListPartnerBySalePerson(string SalePerson)
        {
            try
            {
                var member = db.P_Member.FirstOrDefault(c => c.MemberNumber == SalePerson) ?? new P_Member();
                var partners = db.C_Partner.Where(c => c.Status > 0 &&
                                    (string.IsNullOrEmpty(SalePerson) || 
                                    string.IsNullOrEmpty(member.BelongToPartner) || 
                                    member.BelongToPartner.Contains(c.Code))).OrderBy(c => c.Code).ToList();
                return Json(new object[] { true, partners }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new object[] { false, "Error: " + e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Detail(string Id)
        {
            var partner = db.C_Partner.Find(Id) ?? new C_Partner() { };
            ViewBag.NewPartnerCode = MakeId();
            return PartialView("_Partial_DetailPartner", partner);
        }
        public ActionResult Save(string Id)
        {
            try
            {
                if (access.Any(k => k.Key.Equals("partner_profile")) == false || access["partner_profile"] != true)
                {
                    return Redirect("/home/forbidden");
                }
                var partner = db.C_Partner.Find(Id);
                if (partner == null)
                {
                    TempData["e"] = "Partner not found";
                    return Redirect("/partner");
                }
                ViewBag.access = access;
                ViewBag.NewPartnerCode = MakeId();
                ViewBag.country = db.Ad_Country.ToList();
                ViewBag.states = db.Ad_USAState.ToList();
                var datenow = DateTime.UtcNow.Date;
                ViewBag.credit = db.C_PartnerCard.AsEnumerable().Where(card => card.PartnerCode == partner.Code && CardExpiryDate(card.CardExpiry) >= datenow && card.Active == true).ToList();
                ViewBag.expired_credit = db.C_PartnerCard.AsEnumerable().Where(card => card.PartnerCode == partner.Code && CardExpiryDate(card.CardExpiry) <= datenow && card.Active == true).ToList();
                ViewBag.deactive_credit = db.C_PartnerCard.AsEnumerable().Where(card => card.PartnerCode == partner.Code && card.Active != true).ToList();
                return View(partner);
            }
            catch (Exception ex)
            {
                TempData["e"] = ex.Message;
                return View(new C_Partner { });
            }
        }

        [HttpPost]
        public JsonResult Save(C_Partner _nPartner)
        {
            try
            {
                if (access.Any(k => k.Key.Equals("partner_manage")) == false || access["partner_manage"] != true)
                {
                    throw new Exception("You don't have permission");
                }

                if (string.IsNullOrEmpty(_nPartner.Name)) throw new Exception("Please input Partner name");
                if (string.IsNullOrEmpty(_nPartner.Email)) throw new Exception("Please input Partner email");
                if (db.C_Partner.Where(c => c.Email == _nPartner.Email && c.Id != _nPartner.Id).Count() > 0) throw new Exception("Partner email is available");
                if (db.C_Partner.Where(c => c.Code == _nPartner.Code && c.Id != _nPartner.Id).Count() > 0) throw new Exception("Partner code is available");
                int _nStatus = Request["Active"] != null ? 1 : 0;
                var partner = db.C_Partner.Find(_nPartner.Id);
                if (partner == null) // create new
                {
                    _nPartner.Id = Guid.NewGuid().ToString();
                    //_nPartner.Code = MakeId();
                    _nPartner.CreateAt = DateTime.UtcNow;
                    _nPartner.CreateBy = cMem.FullName;
                    _nPartner.Logo = UploadAttachFile("/upload/partner", "pic0", "", _nPartner.Code + "_" + DateTime.UtcNow.ToString("yyyyMMddhhmmss") + ".png", out string picture);
                    _nPartner.Status = _nStatus;
                    db.C_Partner.Add(_nPartner);
                    db.SaveChanges();

                    //create stage for support, deployment ticket
                    //var tics = db.T_Project_Milestone.Where(c => string.IsNullOrEmpty(c.ParentId) && c.Active == true && (c.BuildInCode == "Deployment_Ticket" || c.BuildInCode == "Support_Ticket")).ToList();
                    //tics.ForEach(t =>
                    //{
                    //    var project = db.T_Project_Milestone.Find(t.Id) ?? new T_Project_Milestone() { };
                    //    var stage = new T_Project_Stage();
                    //    stage.Id = Guid.NewGuid().ToString("N");
                    //    stage.Name = _nPartner.Name;
                    //    stage.Desc = "Stage create after create Partner";
                    //    stage.ProjectId = t.Id;
                    //    stage.ProjectName = project.Name;
                    //    stage.BuildInCode = _nPartner.Code;
                    //    db.T_Project_Stage.Add(stage);
                    //    db.SaveChanges();
                    //});
                    return Json(new object[] { true, "Create Success" });
                }
                else // edit
                {
                    var isDeleteImage = int.Parse(Request["hdPicDelete_pic0"]);
                    if (isDeleteImage == 1) partner.Logo = string.Empty;
                    var oldCode = partner.Code;
                    var oldName = partner.Name;
                    partner.Status = _nStatus;
                    partner.Url = _nPartner.Url;
                    partner.Code = _nPartner.Code;
                    partner.KeyLicense = _nPartner.KeyLicense;
                    partner.Name = _nPartner.Name;
                    partner.Email = _nPartner.Email;
                    partner.Phone = _nPartner.Phone;
                    partner.UpdateBy = cMem.FullName;
                    partner.UpdateAt = DateTime.UtcNow;
                    partner.Hotline = _nPartner.Hotline;
                    partner.Address = _nPartner.Address;
                    partner.ContactName = _nPartner.ContactName;
                    partner.Description = _nPartner.Description;
                    partner.CheckinUrl = _nPartner.CheckinUrl;
                    partner.LoginUrl = _nPartner.LoginUrl;
                    partner.ManageUrl = _nPartner.ManageUrl;
                    partner.PosApiUrl = _nPartner.PosApiUrl;
                    partner.SalesSharePercent = _nPartner.SalesSharePercent;
                    partner.PriceType = _nPartner.PriceType;
                    partner.City = _nPartner.City;
                    partner.State = _nPartner.State;
                    partner.Zipcode = _nPartner.Zipcode;
                    partner.Country = _nPartner.Country;

                    UploadAttachFile("/upload/partner", "pic0", "", _nPartner.Code + "_" + DateTime.UtcNow.ToString("yyyyMMddhhmmss") + ".png", out string picture);
                    partner.Logo = string.IsNullOrEmpty(picture) ? partner.Logo : picture;
                    db.Entry(partner).State = EntityState.Modified;
                    db.SaveChanges();

                    //update deployment, support ticket
                    if (oldCode != _nPartner.Code || oldName != _nPartner.Name)
                    {
                        //var oldStages = db.T_Project_Stage.Where(c => c.BuildInCode == oldCode).ToList();
                        //foreach(var stage in oldStages)
                        //{
                        //    stage.BuildInCode = _nPartner.Code;
                        //    stage.Name = _nPartner.Name;
                        //    db.Entry(stage).State = EntityState.Modified;
                        //    db.SaveChanges();
                        //}
                        //update department
                        var departments = db.P_Department.Where(c => c.PartnerCode == oldCode).ToList();
                        foreach (var d in departments)
                        {
                            d.PartnerCode = _nPartner.Code;
                            d.PartnerName = _nPartner.Name;
                            db.Entry(d).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        var customers = db.C_Customer.Where(c => c.PartnerCode == oldCode).ToList();
                        foreach (var c in customers)
                        {
                            c.PartnerCode = _nPartner.Code;
                            c.PartnerName = _nPartner.Name;
                            db.Entry(c).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }

                    if (oldCode != _nPartner.Code)
                    {
                        //update old order
                        var Orders = db.O_Orders.Where(c => c.PartnerCode == oldCode).ToList();
                        foreach (var order in Orders)
                        {
                            order.PartnerCode = _nPartner.Code;
                            db.Entry(order).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        //update  member
                        var Members = db.P_Member.Where(c => c.BelongToPartner.Contains(oldCode)).ToList();
                        foreach (var memnber in Members)
                        {
                            memnber.BelongToPartner = _nPartner.Code;
                            db.Entry(memnber).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                    return Json(new object[] { true, "Update Success" });
                }
            }
            catch (Exception e)
            {
                return Json(new object[] { false, "Error: " + e.Message });
            }
        }

        public JsonResult Delete(string Id)
        {
            try
            {
                if (access.Any(k => k.Key.Equals("partner_manage")) == false || access["partner_manage"] != true)
                {
                    throw new Exception("You don't have permission");
                }

                var partner = db.C_Partner.Find(Id);
                if (partner == null) throw new Exception("Partner not found");
                partner.Status = -1;
                partner.UpdateAt = DateTime.UtcNow;
                partner.UpdateBy = cMem.FullName;
                db.Entry(partner).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new object[] { true, "Delete Success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new object[] { false, "Error: " + e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private string MakeId()
        {
            return $"P{(int.Parse((db.C_Partner.Where(c => c.Code.Substring(0, 1) == "P").Max(c => c.Code) ?? "P0").Substring(1)) + 1).ToString().PadLeft(5, '0')}";
        }

        public DateTime CardExpiryDate(string CardExpiry)
        {
            try
            {
                int month = int.Parse(CardExpiry.Substring(0, 2));
                int year = int.Parse(CardExpiry.Substring(2, 2)) + 2000;
                return new DateTime(year, month, DateTime.DaysInMonth(year, month));
            }
            catch
            {
                return DateTime.MinValue;
            }
        }
        public JsonResult GetCardInfo(string CardId)
        {
            try
            {
                using (var db = new WebDataModel())
                {
                    var card = db.C_PartnerCard.Find(CardId);
                    return Json(new object[] { true, card, CardExpiryDate(card.CardExpiry) >= DateTime.UtcNow.Date });
                }
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });
            }
        }
        public JsonResult AddNewCard(TransRequest info, string PartnerCode)
        {
            using (var db = new WebDataModel())
            {
                try
                {
                    var partner = db.C_Partner.Where(c => c.Code == PartnerCode).FirstOrDefault();
                    info.MxMerchant_Id = partner.MxMerchant_Id;
                    _payment.NewCard(info, PartnerCode);
                    TempData["s"] = "Add new card completed";
                    return Json(new object[] { true, "Add new card completed" });
                }
                catch (AppHandleException ex)
                {
                    return Json(new object[] { false, "Error: " + ex.Message });
                }
            }
        }
        //public JsonResult SaveEditCard(string partner_Code, C_CustomerCard card)
        //{
        //    try
        //    {
        //        using (var db = new WebDataModel())
        //        {
        //            var partner = db.C_Partner.FirstOrDefault(c => c.Code == partner_Code);
        //            if (partner == null)
        //            {
        //                throw new Exception("Authentication failed");
        //            }
        //            var EditCard = db.C_PartnerCard.Find(card.Id);
        //            if (partner.Code != EditCard.PartnerCode)
        //            {
        //                throw new Exception("Card number is not match!");
        //            }
        //            card.MxMerchant_CardId = EditCard.MxMerchant_CardId;
        //            card.MxMerchant_Id = EditCard.MxMerchant_Id;
        //            card.MxMerchant_Token = EditCard.MxMerchant_Token;
        //            _payment.EditCard(card, partner_Code);
        //            return Json(new object[] { true, EditCard.Active, Check_expiry(EditCard.CardExpiry) });
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        return Json(new object[] { false, e.Message });
        //    }
        //}
        private bool Check_expiry(string cardExpiry)
        {
            try
            {
                int month = int.Parse(cardExpiry.Substring(0, 2));
                int year = int.Parse(cardExpiry.Substring(2, 2)) + 2000;
                if (new DateTime(year, month, DateTime.DaysInMonth(year, month)) >= DateTime.UtcNow.Date)
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        public JsonResult ChangeDefaultCard(string cardId)
        {
            try
            {
                using (var db = new WebDataModel())
                {
                    //new default card
                    C_PartnerCard newdefault_card = db.C_PartnerCard.Find(cardId);
                    if (newdefault_card == null) throw new Exception("Card not found");
                    //current default card
                    var current_card = db.C_PartnerCard.FirstOrDefault(c => c.PartnerCode == newdefault_card.PartnerCode && c.IsDefault == true);
                    //inactive all services Active of current default card
                    db.Store_Services.Where(s => s.MxMerchant_SubscriptionStatus == "Active" && s.MxMerchant_cardAccountId == current_card.MxMerchant_Id).ToList().ForEach(s =>
                    {
                        PaymentService.SetStatusRecurring(s.Id, "inactive");
                    });
                    newdefault_card.IsDefault = true;
                    newdefault_card.IsRecurring = true;
                    current_card.IsDefault = false;
                    current_card.IsRecurring = false;
                    db.Entry(newdefault_card).State = EntityState.Modified;
                    db.Entry(current_card).State = EntityState.Modified;
                    db.SaveChanges();
                    //active all services Inactive of new default card
                    db.Store_Services.Where(s => s.MxMerchant_SubscriptionStatus == "Inactive" && s.MxMerchant_cardAccountId == newdefault_card.MxMerchant_Id).ToList().ForEach(s =>
                    {
                        PaymentService.SetStatusRecurring(s.Id, "active");
                    });
                    TempData["s"] = "Update success";
                    return Json(new object[] { true });
                }
            }
            catch (Exception e)
            {
                TempData["e"] = "Update error: " + e.Message;
                return Json(new object[] { false, e.Message });
            }
        }
        public JsonResult ChangeSubscriptionRenew_byCard(long MxCardId, bool isActive)
        {
            try
            {
                //if (access.Any(k => k.Key.Equals("customer_mango_control")) != true || access["customer_mango_control"] != true)
                //{
                //    throw new Exception("You dont have permission to change!");
                //}
                var cardData = db.C_PartnerCard.FirstOrDefault(c => c.MxMerchant_Id == MxCardId);
                if (cardData == null) throw new Exception("Card not found");
                var statusFilter = isActive ? "Inactive" : "Active";
                var statusChange = isActive ? "active" : "inactive";
                var store_item = db.Store_Services.Where(s => s.MxMerchant_SubscriptionStatus == statusFilter && s.MxMerchant_cardAccountId == MxCardId).ToList();
                if (isActive != true)
                {
                    store_item.ForEach(s =>
                    {
                        PaymentService.SetStatusRecurring(s.Id, statusChange);
                    });
                }
                cardData.IsRecurring = isActive;
                db.Entry(cardData).State = EntityState.Modified;
                db.SaveChanges();
                if (isActive == true)
                {
                    store_item.ForEach(s =>
                    {
                        PaymentService.SetStatusRecurring(s.Id, statusChange);
                    });
                }
                TempData["s"] = "Update success";
                return Json(new object[] { true, isActive ? "Turn on recurring completed" : "Turn off recurring completed" });
            }
            catch (Exception e)
            {
                TempData["e"] = "Update error: " + e.Message;
                return Json(new object[] { false, e.Message });
            }
        }
        public ActionResult ChangeStatusCard(string id, bool active)
        {
            try
            {
                using (var db = new WebDataModel())
                {
                    var card = db.C_PartnerCard.Find(id);
                    if (card == null)
                    {
                        throw new Exception("Card not found!");
                    }
                    if (card.IsDefault && !active)
                    {
                        throw new Exception("Can't deactivate Default card!");
                    }
                    card.Active = active;
                    db.Entry(card).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["s"] = "Update success";
                    return Json(new object[] { true, active, CardExpiryDate(card.CardExpiry) >= DateTime.UtcNow.Date });

                }
            }
            catch (Exception e)
            {
                TempData["e"] = "Update error: " + e.Message;
                return Json(new object[] { false, e.Message });
            }

        }
    }
}
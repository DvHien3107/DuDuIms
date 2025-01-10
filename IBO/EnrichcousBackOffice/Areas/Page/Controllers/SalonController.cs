using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.ViewControler;
using EnrichcousBackOffice.AppLB;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Configuration;
using System.Data.Entity;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Security;
using EnrichcousBackOffice.Services;
using EnrichcousBackOffice.ViewModel;
using Newtonsoft.Json;
using EnrichcousBackOffice.Utils.IEnums;
using Inner.Libs.Helpful;
using EnrichcousBackOffice.Utils.AppConfig;
using EnrichcousBackOffice.Areas.PaymentGate.Services;
using EnrichcousBackOffice.Areas.Page.Models;
using EnrichcousBackOffice.Models.CustomizeModel;
using Enrich.IServices.Utils.Mailing;
using Enrich.DataTransfer;
using EnrichcousBackOffice.NextGen;
using Enrich.Core.Ultils;

namespace EnrichcousBackOffice.Areas.Page.Controllers
{
    public class SalonController : EnrichcousBackOffice.Controllers.UploadController
    {
        private WebDataModel db = new WebDataModel();
        private readonly IMailingService _mailingService;

        public SalonController(IMailingService mailingService)
        {
            _mailingService = mailingService;
        }

        // GET: Questionare/Salon
        public ActionResult Index()
        {
            return RedirectToAction("err404");
        }

        public ActionResult err404()
        {
            return View();
        }

        [HttpPost]
        public ActionResult QuestionnaireLogin()
        {
            using (WebDataModel _db = new WebDataModel())
            {
                var customer_id = long.Parse(Request["customer_id"]);
                var email = Request["email"];
                var password = Request["password"];
                var customer_name = Request["customer_name"];
                var update = Request["update"] == "true";
                try
                {
                    C_Customer.Login(email, password, Session);
                    return RedirectToAction("Questionare", "Salon", new { id = customer_id, n = customer_name, u = update });
                }
                catch (Exception ex)
                {
                    TempData["error"] = ex.Message;
                    return RedirectToAction("Questionare", "Salon", new { id = customer_id, n = customer_name, u = update });
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Customer Id</param>
        /// <param name="n">Businees Name</param>
        /// <param name="u">Update: true/false</param>
        /// <returns></returns>
        public ActionResult Questionare(long? id, string n, bool u = false)
        {
            try
            {
                if (string.IsNullOrEmpty(Session["error_msg"] as string) == false)
                {
                    TempData["error"] = Session["error_msg"];
                    Session["error_msg"] = null;
                }
                ViewBag.CustomerId = id;
                ViewBag.CustomerName = n;
                ViewBag.Update = u;
                var cus = (from c in db.C_Customer.ToList()
                           where c.Id == id
                           select c).FirstOrDefault();

                if (cus != null)
                {
                    ViewBag.Customer = cus;
                    var questionare = db.C_Questionare.FirstOrDefault(x => x.CustomerId == id);
                    var list_nonTech = new List<QuestNonTechViewService>();
                    var list_Tech = new List<QuestTechViewService>();

                    if (questionare == null)
                    {
                        /*
                        TODO : Customer login
                        string authInfo = Session["Realm"]?.ToString() ?? null;
                        if (!(authInfo != null && authInfo.StartsWith("Basic")))
                        {
                            ViewBag.Authorization = false;
                        }
                        else if (authInfo.StartsWith("Basic"))
                        {
                            string credentials = authInfo.Substring("Basic ".Length).Trim();
                            Encoding encoding = Encoding.GetEncoding("ISO-8859-1");
                            string basicInfo = encoding.GetString(Convert.FromBase64String(credentials));
                            ViewBag.Authorization = basicInfo.StartsWith(id.ToString());
                        }

                        if (ViewBag.Authorization == false && (ViewBag.Start ?? true) == false)
                        {
                            Session["Bear"] = null;
                            TempData["error"] = "Email or password is incorrect!";
                            ViewBag.Start = false;
                        }
                        ViewBag.Start = false;
                        */

                        //khach hang chua co questionare => tao moi
                        ViewBag.MoreFiles = new List<UploadMoreFile>();
                        ViewBag.ListNonTechnician = list_nonTech;
                        ViewBag.ListTechnician = list_Tech;

                        return View(new C_Questionare());
                    }
                    else
                    {
                        var list_morefile = db.UploadMoreFiles.Where(f => f.TableId == questionare.CustomerId && f.TableName.Equals("C_Questionare")).ToList();
                        ViewBag.MoreFiles = list_morefile.Where(x => x.FileName.Contains("img_snap") == false).ToList();
                        ViewBag.MoreFilesSnap = list_morefile.Where(x => x.FileName.Contains("img_snap") == true).ToList();
                        if (u == true)
                        {
                            /*
                            TODO : Customer login
                            string authInfo = Session["Realm"]?.ToString() ?? null;
                            if (!(authInfo != null && authInfo.StartsWith("Basic")))
                            {
                                ViewBag.Authorization = false;
                            }
                            else if (authInfo.StartsWith("Basic"))
                            {
                                string credentials = authInfo.Substring("Basic ".Length).Trim();
                                Encoding encoding = Encoding.GetEncoding("ISO-8859-1");
                                string basicInfo = encoding.GetString(Convert.FromBase64String(credentials));
                                ViewBag.Authorization = basicInfo.StartsWith(id.ToString());
                            }
                            if (ViewBag.Authorization == false && (ViewBag.Start ?? true) == false)
                            {
                                Session["Bear"] = null;
                                TempData["error"] = "Email or password is incorrect!";
                                ViewBag.Start = false;
                            }
                            ViewBag.Start = false;
                            */

                            //khach hang da co questionare va update == true => update
                            var nonTech = db.C_Questionare_NonTechnician.Where(x => x.QuestionareId == questionare.Id).OrderBy(x => x.CreateAt).ToList();
                            foreach (var item in nonTech)
                            {
                                var nonTechViewController = new QuestNonTechViewService()
                                {
                                    Key = nonTech.IndexOf(item) + 1,
                                    Id = item.Id,
                                    QuestionareId = item.QuestionareId,
                                    Name = item.Name,
                                    Job = item.Job,
                                    Pay = item.Pay,
                                    AdjustViewPayroll = item.AdjustViewPayroll
                                };
                                list_nonTech.Add(nonTechViewController);
                            }
                            ViewBag.ListNonTechnician = list_nonTech;


                            var Tech = db.C_Questionare_Technician.Where(x => x.QuestionareId == questionare.Id).OrderBy(x => x.CreateAt).ToList();
                            foreach (var item in Tech)
                            {
                                var TechViewController = new QuestTechViewService()
                                {
                                    Key = Tech.IndexOf(item) + 1,
                                    Id = item.Id,
                                    QuestionareId = item.QuestionareId,
                                    Name = item.Name,
                                    NickName = item.NickName,
                                    Commission = item.Commission,
                                    PayrollSplitCheckOrCash = item.PayrollSplitCheckOrCash,
                                    AddDiscounts = item.AddDiscounts,
                                    AdjustPrices = item.AdjustPrices
                                };
                                list_Tech.Add(TechViewController);
                            }
                            ViewBag.ListTechnician = list_Tech;

                            return View(questionare);
                        }
                        else
                        {
                            //update == false => tra ve trang result
                            return RedirectToAction("Result", new { id = questionare.Id });
                        }
                    }
                }
                else
                {
                    return RedirectToAction("err404");
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return Redirect("/account/login");
            }
        }

        public ActionResult GiftCardsOrderingForm(string Key,bool isUpdate = false)
        {
            if (string.IsNullOrEmpty(Key))
            {
                return RedirectToAction("err404");
            }
            long? Id = long.Parse(SecurityLibrary.Decrypt(Key));
            var GiftCardOrderFormEntity = db.C_GiftCardsOrderingForm.Find(Id);
            if (GiftCardOrderFormEntity == null)
            {
                return RedirectToAction("err404");
            }
            if(!string.IsNullOrEmpty(GiftCardOrderFormEntity.ProductCode)&& isUpdate==false)
            {
                return Redirect("/Page/Salon/GiftCardsOrderingFormView?Key=" + Key);
            }
            var customer = db.C_Customer.Where(x=>x.CustomerCode == GiftCardOrderFormEntity.CustomerCode).FirstOrDefault();
            if (customer == null)
            {
                return RedirectToAction("err404");
            }
            var GiftCardOrderFormModel = new GiftCardsOrderingFormModel();
            //set data from customer
            GiftCardOrderFormModel.CustomerId = customer.Id;
            GiftCardOrderFormModel.ContactName = customer.ContactName;
            GiftCardOrderFormModel.ContactPhone = customer.CellPhone;
            GiftCardOrderFormModel.ContactEmail = customer.Email??customer.SalonEmail; 
            GiftCardOrderFormModel.BusinessName = customer.BusinessName;
            GiftCardOrderFormModel.SalonAddress1 = customer.SalonAddress1;
            GiftCardOrderFormModel.SalonAddress2 = customer.SalonAddress2;
            GiftCardOrderFormModel.SalonCity = customer.SalonCity;
            GiftCardOrderFormModel.SalonState = customer.SalonState;
            GiftCardOrderFormModel.ZipCode = customer.SalonZipcode;
            //set data from gift card form
            GiftCardOrderFormModel.Id = GiftCardOrderFormEntity.Id;
            GiftCardOrderFormModel.SalonHours = GiftCardOrderFormEntity.SalonHours; 
            GiftCardOrderFormModel.Note = GiftCardOrderFormEntity.Note;
            GiftCardOrderFormModel.ProductCode = GiftCardOrderFormEntity.ProductCode;
            GiftCardOrderFormModel.BackDesign = GiftCardOrderFormEntity.BackDesign;
            GiftCardOrderFormModel.FrontDesign = GiftCardOrderFormEntity.FrontDesign;
            GiftCardOrderFormModel.FrontDesignFiles = GiftCardOrderFormEntity.FrontDesignFiles;
            GiftCardOrderFormModel.BackDesignFiles = GiftCardOrderFormEntity.BackDesignFiles;
            GiftCardOrderFormModel.ListState = db.Ad_USAState.Where(x => x.country == "USA").Select(x=>new SelectListItem {
            Value = x.abbreviation,
            Text = x.name
            }).ToList();
            var mGiftCard = LicenseType.GiftCard.Text();
            GiftCardOrderFormModel.ListProduct = db.License_Product.Where(x => x.Type == mGiftCard && x.Active == true).OrderBy(x=>x.Level).Select(x => new SelectListItem { 
                Value = x.Code,
                Text= x.Name, 
            }).ToList();
            return View(GiftCardOrderFormModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> GiftCardsOrderingForm(GiftCardsOrderingFormModel model)
        {
           
            var GiftCardOrderFormEntity = db.C_GiftCardsOrderingForm.Find(model.Id);
            if (GiftCardOrderFormEntity == null)
            {
                return RedirectToAction("err404");
            }
            var customer = db.C_Customer.Where(x => x.CustomerCode == GiftCardOrderFormEntity.CustomerCode).FirstOrDefault();
            if (customer == null)
            {
                return RedirectToAction("err404");
            }
            //update gift card
            GiftCardOrderFormEntity.FrontDesign = model.FrontDesign;
            GiftCardOrderFormEntity.BackDesign = model.BackDesign;
            GiftCardOrderFormEntity.Note = model.Note;
            GiftCardOrderFormEntity.ProductCode = model.ProductCode;
            GiftCardOrderFormEntity.SalonHours = model.SalonHours;
            GiftCardOrderFormEntity.UpdateAt = DateTime.UtcNow;

            // update customer
            customer.OwnerName = customer.ContactName = model.ContactName;
            customer.OwnerMobile = customer.CellPhone = model.ContactPhone;
            customer.Email = customer.Email = model.ContactEmail;
            customer.BusinessName = model.BusinessName;
            customer.SalonAddress1 = model.SalonAddress1;
            customer.SalonAddress2 = model.SalonAddress2;
            customer.SalonCity = customer.BusinessCity = model.SalonCity;
            customer.SalonState = customer.BusinessState = model.SalonState;
            customer.SalonZipcode = customer.BusinessZipCode = model.ZipCode;
            string Path = "/Upload/GiftCardsOrderingForm/" + GiftCardOrderFormEntity.Id;
            string absolutePath = Server.MapPath(Path);
            bool exists = System.IO.Directory.Exists(absolutePath);
            if (!exists)
                System.IO.Directory.CreateDirectory(absolutePath);
            if (Request.Files["FrontDesignFiles"] != null)
            {
                var FrontDesignFiles = Request.Files["FrontDesignFiles"];
                if (FrontDesignFiles.ContentLength > 0)
                {
                    FrontDesignFiles.SaveAs(absolutePath + "/" + FrontDesignFiles.FileName);
                    GiftCardOrderFormEntity.FrontDesignFiles = Path + "/" + FrontDesignFiles.FileName;
                }
            }
            if (Request.Files["BackDesignFiles"] != null)
            {
                var BackDesignFiles = Request.Files["BackDesignFiles"];
                if (BackDesignFiles.ContentLength > 0)
                {
                    BackDesignFiles.SaveAs(absolutePath + "/" + BackDesignFiles.FileName);
                    GiftCardOrderFormEntity.BackDesignFiles = Path + "/" + BackDesignFiles.FileName;
                }
            }
            db.SaveChanges();
            var Key = SecurityLibrary.Encrypt(GiftCardOrderFormEntity.Id.ToString());
            string url = "/Page/Salon/GiftCardsOrderingFormView?Key=" + Key;
            #region send email to sales person
            var salesperson = db.P_Member.Where(x => x.MemberNumber == GiftCardOrderFormEntity.SalesPerson).FirstOrDefault();
            var emailData = new SendGridEmailTemplateData.NotifySubmitGiftCard()
            {
                owner_name = customer.OwnerName,
                sales_person = salesperson.FullName,
                link = ConfigurationManager.AppSettings["IMSUrl"] + url
            };
            ReadXML xml = new ReadXML(System.Web.Hosting.HostingEnvironment.MapPath("/App_Data/Config.xml"));
            XmlNode node = xml.GetNode("/root/sendgrid_template/notify_sales_giftcards");
            var SystemConfigurations = db.SystemConfigurations.FirstOrDefault();
            string cc = string.Join(";",  SystemConfigurations.SalesEmail, SystemConfigurations.SupportEmail);
            await _mailingService.SendBySendGridWithTemplate(salesperson.PersonalEmail, "", node["template_id"].InnerText, cc, emailData, "");
            #endregion
       
            return Redirect(url);
        }

        public ActionResult GiftCardsOrderingFormView (string Key)
        {
            if (string.IsNullOrEmpty(Key))
            {
                return RedirectToAction("err404");
            }
            long? Id = long.Parse(SecurityLibrary.Decrypt(Key));
            var GiftCardOrderFormEntity = db.C_GiftCardsOrderingForm.Find(Id);
            if (GiftCardOrderFormEntity == null|| string.IsNullOrEmpty(GiftCardOrderFormEntity.ProductCode))
            {
                return RedirectToAction("err404");
            }
            var customer = db.C_Customer.Where(x => x.CustomerCode == GiftCardOrderFormEntity.CustomerCode).FirstOrDefault();
            if (customer == null)
            {
                return RedirectToAction("err404");
            }
            var GiftCardOrderFormModel = new GiftCardsOrderingFormModel();
            GiftCardOrderFormModel.CustomerId = customer.Id;
            GiftCardOrderFormModel.ContactName = customer.ContactName;
            GiftCardOrderFormModel.ContactPhone = customer.CellPhone;
            GiftCardOrderFormModel.ContactEmail = customer.Email ?? customer.SalonEmail;
            GiftCardOrderFormModel.BusinessName = customer.BusinessName;
            GiftCardOrderFormModel.SalonAddress = customer.SalonAddress1 + ", " + customer.SalonCity + ", " + customer.SalonState + " " + customer.SalonZipcode + ", " + customer.BusinessCountry;
            GiftCardOrderFormModel.SalonHours = GiftCardOrderFormEntity.SalonHours;
            GiftCardOrderFormModel.ProductName = db.License_Product.Where(x => x.Code == GiftCardOrderFormEntity.ProductCode).FirstOrDefault().Name;
            GiftCardOrderFormModel.FrontDesign = GiftCardOrderFormEntity.FrontDesign;
            GiftCardOrderFormModel.BackDesign = GiftCardOrderFormEntity.BackDesign;
            GiftCardOrderFormModel.Note = GiftCardOrderFormEntity.Note;
            GiftCardOrderFormModel.FrontDesignFiles = GiftCardOrderFormEntity.FrontDesignFiles;
            GiftCardOrderFormModel.BackDesignFiles = GiftCardOrderFormEntity.BackDesignFiles;

            ViewBag.Key = Key;
            return View(GiftCardOrderFormModel);
        }
        //[HttpPost]
        //public ActionResult UploadDesignFile(string Type)
        //{
        //    HttpPostedFileBase httpPostedFile = Request.Files[0];
        //    return Json(true);
        //}

        [HttpPost]
        public async Task<ActionResult> Questionare(C_Questionare fDataModal)
        {
            // using (var trans = db.Database.BeginTransaction())
            {
                try
                {
                    var today = DateTime.UtcNow;
                    var img_snap = Request["imageprev"];
                    var cusId = long.Parse(Request["cusId"].ToString());
                    var cusCode = Request["cusCode"];
                    //var a =Request.Files["Drivers_License_Front_Image"].FileName;
                    string Path = "/Upload/Questionare/" + cusId;
                    string absolutePath = Server.MapPath(Path);
                    bool exists = System.IO.Directory.Exists(absolutePath);
                    if (!exists)
                        System.IO.Directory.CreateDirectory(absolutePath);
                    if (string.IsNullOrEmpty(fDataModal.Id) == true)
                    {
                        //Add
                        #region Create new Questionare
                        var new_ques = fDataModal;
                        new_ques.Id = System.Guid.NewGuid().ToString();
                        new_ques.CustomerId = cusId;
                        new_ques.CustomerCode = cusCode;
                        new_ques.AllowTexting = Request["AllowTexting"] == "1" ? true : false;
                        new_ques.EthernetWrite = Request["EthernetWire"] == "1" ? true : false;
                        new_ques.OwnerEmail = fDataModal.OwnerEmail;
                        new_ques.SalonEmail = fDataModal.SalonEmail;
                        new_ques.SalonAddress1 = fDataModal.SalonAddress1;
                        new_ques.SalonAddress2 = fDataModal.SalonAddress2;
                        new_ques.SalonCity = fDataModal.SalonCity;
                        new_ques.SalonState = fDataModal.SalonState;
                        new_ques.SalonZipcode = fDataModal.SalonZipcode;
                        new_ques.SalonPhone = fDataModal.SalonPhone;
                        new_ques.SalonTimeZone = fDataModal.SalonTimeZone;
                        new_ques.SendToOwnersEmail = fDataModal.SendToOwnersEmail;
                     
                        if (Request.Files["Drivers_License_Front_Image"] != null)
                        {
                            var Drivers_License_Front_Image = Request.Files["Drivers_License_Front_Image"];
                            if (Drivers_License_Front_Image.ContentLength > 0)
                            {
                                Drivers_License_Front_Image.SaveAs(absolutePath + "/" + Drivers_License_Front_Image.FileName);
                                new_ques.Drivers_License_Front_Image = Path + "/" + Drivers_License_Front_Image.FileName;
                            }
                        }
                        if (Request.Files["Drivers_License_Back_Image"] != null)
                        {
                            var Drivers_License_Back_Image = Request.Files["Drivers_License_Back_Image"];
                            if (Drivers_License_Back_Image.ContentLength > 0)
                            {
                                Drivers_License_Back_Image.SaveAs(absolutePath + "/" + Drivers_License_Back_Image.FileName);
                                new_ques.Drivers_License_Back_Image = Path + "/" + Drivers_License_Back_Image.FileName;
                            }
                        }
                        if (Request.Files["VoidedBusinessCheck"] != null)
                        {
                            var VoidedBusinessCheck = Request.Files["VoidedBusinessCheck"];
                            if (VoidedBusinessCheck.ContentLength > 0)
                            {
                                VoidedBusinessCheck.SaveAs(absolutePath + "/" + VoidedBusinessCheck.FileName);
                                new_ques.VoidedBusinessCheck = Path + "/" + VoidedBusinessCheck.FileName;
                            }
                        }
                        if (Request.Files["SS4"] != null)
                        {
                            var SS4 = Request.Files["SS4"];
                            if (SS4.ContentLength > 0)
                            {
                                SS4.SaveAs(absolutePath + "/" + SS4.FileName);
                                new_ques.SS4 = Path + "/" + SS4.FileName;
                            }
                        }
                        //if (string.IsNullOrEmpty(fDataModal.DiscountType) == false)
                        //{
                        //    new_ques.TechCommDiscount = Request["TechCommDiscounts"] == "1" ? true : false;
                        //}
                        new_ques.ComboSpecialDiscount = Request["ComboSpecialDiscount"] == "1" ? true : false;

                        if (Request["PayoutCreditCard"] != null)
                        {
                            new_ques.PayoutCreditCard = Request["PayoutCreditCard"] == "1" ? true : false;
                        }

                        if (Request["TechSellGiftCard"] == "1")
                        {
                            new_ques.AUTHORIZED_Seller = fDataModal.AUTHORIZED_Seller.Replace("\n", "<br/>");
                        }
                        else
                        {
                            new_ques.AUTHORIZED_Seller = null;
                        }

                        if (Request["TechMakeAppointment"] == "1")
                        {
                            new_ques.AUTHORIZED_Tech = fDataModal.AUTHORIZED_Tech.Replace("\n", "<br/>");
                        }
                        else
                        {
                            new_ques.AUTHORIZED_Tech = null;
                        }
                        new_ques.CustomerAddTips = fDataModal.CustomerAddTips;
                        if (new_ques.CustomerAddTips == true)
                        {
                            new_ques.SuggestPercentTip = fDataModal.SuggestPercentTip;
                        }
                        else
                        {
                            new_ques.SuggestPercentTip = null;
                        }
                        new_ques.ChargeFeeTips = fDataModal.ChargeFeeTips;
                        new_ques.CommissionProductSales = fDataModal.CommissionProductSales;
                        new_ques.DateSubmit = today;
                        new_ques.CreateAt = today;

                        db.C_Questionare.Add(new_ques);
                        #endregion

                        #region Add list nonTechnician 

                        //var list_nonTech = Session[LIST_NONTECHNICIAN] as List<QuestNonTechViewController>;
                        var list_nonTech = AddNonTechItem(int.Parse(Request["hd_managerItem"]));
                        foreach (var item in list_nonTech)
                        {
                            var nonTech = new C_Questionare_NonTechnician()
                            {
                                Id = System.Guid.NewGuid().ToString(),
                                QuestionareId = new_ques.Id,
                                Name = item.Name,
                                Job = item.Job,
                                Pay = item.Pay,
                                AdjustViewPayroll = item.AdjustViewPayroll,
                                CreateAt = DateTime.UtcNow,
                            };
                            db.C_Questionare_NonTechnician.Add(nonTech);
                        }
                        #endregion

                        #region Add list Technician
                        var list_Tech = AddTechItem(int.Parse(Request["hd_techItem"]));
                        foreach (var item in list_Tech)
                        {
                            var tech = new C_Questionare_Technician()
                            {
                                Id = System.Guid.NewGuid().ToString(),
                                QuestionareId = new_ques.Id,
                                Name = item.Name,
                                NickName = item.NickName,
                                Commission = item.Commission,
                                PayrollSplitCheckOrCash = item.PayrollSplitCheckOrCash,
                                AddDiscounts = item.AddDiscounts,
                                AdjustPrices = item.AdjustPrices,
                                CreateAt = DateTime.UtcNow
                            };
                            db.C_Questionare_Technician.Add(tech);
                        }
                        #endregion

                        #region update merchant
                        var cus = db.C_Customer.Where(c => c.CustomerCode == new_ques.CustomerCode).FirstOrDefault();
                        cus.BusinessName = new_ques.SalonName;
                        cus.OwnerName = new_ques.ContactName;
                        cus.OwnerMobile = new_ques.ContactNumber;
                        cus.Email = new_ques.OwnerEmail;

                        if (string.IsNullOrWhiteSpace(cus.BusinessAddressStreet))
                        {
                            cus.BusinessAddressStreet = new_ques.SalonAddress1 + " " + new_ques.SalonAddress2;
                        }
                        if (string.IsNullOrWhiteSpace(cus.BusinessCity))
                        {
                            cus.BusinessCity = new_ques.SalonCity;
                        }
                        if (string.IsNullOrWhiteSpace(cus.BusinessState))
                        {
                            cus.BusinessState = new_ques.SalonState;
                        }
                        if (string.IsNullOrWhiteSpace(cus.BusinessZipCode))
                        {
                            cus.BusinessZipCode = new_ques.SalonZipcode;
                        }

                        cus.SalonEmail = new_ques.SalonEmail;
                        cus.SalonPhone = new_ques.SalonPhone;
                        cus.SalonTimeZone = new_ques.SalonTimeZone;
                        cus.SalonTimeZone_Number = Ext.EnumParse<TIMEZONE_NUMBER>(new_ques.SalonTimeZone).Text();
                        cus.SalonAddress1 = new_ques.SalonAddress1;
                        cus.SalonAddress2 = new_ques.SalonAddress2;
                        cus.SalonCity = new_ques.SalonCity;
                        cus.SalonState = new_ques.SalonState;
                        cus.SalonZipcode = new_ques.SalonZipcode;
                        var countryDefault = "United States";
                        if (string.IsNullOrEmpty(cus.Country)) cus.Country = countryDefault;
                        if (string.IsNullOrEmpty(cus.BusinessCountry)) cus.BusinessCountry = countryDefault;

                        db.Entry(cus).State = System.Data.Entity.EntityState.Modified;
                        #endregion

                        await db.SaveChangesAsync();
                        if (db.Store_Services.Any(s => s.StoreCode == cus.StoreCode && s.Active == 1) && !string.IsNullOrEmpty(cus.Version))
                        {
                            //PushStoreToMangoPos
                            var active_service_id = db.Store_Services.FirstOrDefault(s => s.StoreCode == cus.StoreCode && s.Active == 1)?.Id;
                            using (MerchantService service = new MerchantService(true))
                            {
                                var rs = await service.ApproveAction(cus.StoreCode, active_service_id, true, "same-active");
                            }
                            PaymentService.UpdateRecurringStatus();
                        }

                        //Save attach file
                        int filescount = Request.Files.Count;
                        await  UploadMultipleFilesAsync(new_ques.CustomerId ?? 0, "C_Questionare", filescount, "/upload/other");

                        //Save image snapshot file
                        //int file_snap_count = int.Parse(Request["file-snap-count"].ToString());
                        //UploadMoreFiles(new_ques.CustomerId ?? 0, "C_Questionare", file_snap_count, "/upload/img_snap");

                        //Send email cho nhan vien
                        await SendNotice_Questionnaire(new_ques);

                        // trans.Commit();
                        TempData["s"] = "Submit questionnaire success";
                        return RedirectToAction("Result", new { id = new_ques.Id });
                    }
                    else
                    {
                        //Update
                        #region Update Questionare
                        var questionare = db.C_Questionare.Find(fDataModal.Id);
                        questionare.SalonName = fDataModal.SalonName;
                        questionare.ContactName = fDataModal.ContactName;
                        questionare.ContactNumber = fDataModal.ContactNumber;
                        //questionare.SalesRep = fDataModal.SalesRep;
                        questionare.OwnerEmail = fDataModal.OwnerEmail;
                        questionare.SalonEmail = fDataModal.SalonEmail;
                        questionare.AllowTexting = Request["AllowTexting"] == "1" ? true : false;
                        questionare.SalonHours = fDataModal.SalonHours;
                        questionare.EthernetWrite = Request["EthernetWire"] == "1" ? true : false;
                        //questionare.PromotionType = Request["PromotionDiscount"] == "1" ? fDataModal.PromotionType : "";
                        questionare.ServiceFee = Request["ServicesCharge_SupplyFee"] == "1" ? fDataModal.ServiceFee : "";

                        questionare.SalonAddress1 = fDataModal.SalonAddress1;
                        questionare.SalonAddress2 = fDataModal.SalonAddress2;
                        questionare.SalonCity = fDataModal.SalonCity;
                        questionare.SalonState = fDataModal.SalonState;
                        questionare.SalonZipcode = fDataModal.SalonZipcode;
                        questionare.SalonPhone = fDataModal.SalonPhone;
                        questionare.SalonTimeZone = fDataModal.SalonTimeZone;
                        questionare.SendToOwnersEmail = fDataModal.SendToOwnersEmail;
               
                        if (Request.Files["Drivers_License_Front_Image"] != null)
                        {
                            var Drivers_License_Front_Image = Request.Files["Drivers_License_Front_Image"];
                            if(Drivers_License_Front_Image.ContentLength > 0)
                            {
                                Drivers_License_Front_Image.SaveAs(absolutePath + "/" + Drivers_License_Front_Image.FileName);
                                questionare.Drivers_License_Front_Image = Path + "/" + Drivers_License_Front_Image.FileName;
                            }
                        }
                        if (Request.Files["Drivers_License_Back_Image"] != null)
                        {
                            var Drivers_License_Back_Image = Request.Files["Drivers_License_Back_Image"];
                            if (Drivers_License_Back_Image.ContentLength > 0)
                            {
                                Drivers_License_Back_Image.SaveAs(absolutePath + "/" + Drivers_License_Back_Image.FileName);
                                questionare.Drivers_License_Back_Image = Path + "/" + Drivers_License_Back_Image.FileName;
                            }
                        }
                        if (Request.Files["VoidedBusinessCheck"] != null)
                        {
                            var VoidedBusinessCheck = Request.Files["VoidedBusinessCheck"];
                            if (VoidedBusinessCheck.ContentLength > 0)
                            {
                                VoidedBusinessCheck.SaveAs(absolutePath + "/" + VoidedBusinessCheck.FileName);
                                questionare.VoidedBusinessCheck = Path + "/" + VoidedBusinessCheck.FileName;
                            }
                        }
                        if (Request.Files["SS4"] != null)
                        {
                            var SS4 = Request.Files["SS4"];
                            if (SS4.ContentLength > 0)
                            {
                                SS4.SaveAs(absolutePath + "/" + SS4.FileName);
                                questionare.SS4 = Path + "/" + SS4.FileName;
                            }
                        }
                       
                        //if (Request["GeneralDiscount"] == "1" && string.IsNullOrEmpty(fDataModal.DiscountType) == false)
                        //{
                        //    questionare.DiscountType = fDataModal.DiscountType;
                        //    questionare.TechCommDiscount = Request["TechCommDiscounts"] == "1" ? true : false;
                        //}
                        //else
                        //{
                        //    questionare.DiscountType = null;
                        //    questionare.TechCommDiscount = null;
                        //}
                        questionare.TechOrder = fDataModal.TechOrder;
                        questionare.DollarAmount = fDataModal.DollarAmount;
                        questionare.ComboSpecialDiscount = Request["ComboSpecialDiscount"] == "1" ? true : false;
                        questionare.Note = fDataModal.Note;
                        if (Request["PayoutCreditCard"] != null)
                        {
                            questionare.PayoutCreditCard = Request["PayoutCreditCard"] == "1" ? true : false;
                        }

                        if (Request["PayoutCreditCard"] == null || questionare.PayoutCreditCard == true)
                        {
                            questionare.PayoutCreditCard = Request["PayoutCreditCard"] == null ? null : questionare.PayoutCreditCard;
                        }

                        if (Request["TechSellGiftCard"] == "1")
                        {
                            questionare.AUTHORIZED_Seller = fDataModal.AUTHORIZED_Seller.Replace("\n", "<br/>");
                        }
                        else
                        {
                            questionare.AUTHORIZED_Seller = null;
                        }

                        if (Request["TechMakeAppointment"] == "1")
                        {
                            questionare.AUTHORIZED_Tech = fDataModal.AUTHORIZED_Tech.Replace("\n", "<br/>");
                        }
                        else
                        {
                            questionare.AUTHORIZED_Tech = null;
                        }
                        questionare.OtherRequest = fDataModal.OtherRequest;
                        questionare.LastUpdate = today;
                        if (string.IsNullOrEmpty(questionare.UpdateHistory) == true)
                        {
                            questionare.UpdateHistory = today.ToString();
                        }
                        else
                        {
                            questionare.UpdateHistory = questionare.UpdateHistory + "|" + today.ToString();
                        }
                        questionare.CustomerAddTips = fDataModal.CustomerAddTips;
                        if (questionare.CustomerAddTips == true)
                        {
                            questionare.SuggestPercentTip = fDataModal.SuggestPercentTip;
                        }
                        else
                        {
                            questionare.SuggestPercentTip = null;
                        }
                        questionare.ChargeFeeTips = fDataModal.ChargeFeeTips;
                        questionare.CommissionProductSales = fDataModal.CommissionProductSales;

                        db.Entry(questionare).State = System.Data.Entity.EntityState.Modified;
                        #endregion

                        #region Update nonTechnician
                        //var list_nonTech_ss = Session[LIST_NONTECHNICIAN] as List<QuestNonTechViewController>;
                        var list_nonTech_ss = AddNonTechItem(int.Parse(Request["hd_managerItem"]));
                        var list_nonTech_db = db.C_Questionare_NonTechnician.Where(x => x.QuestionareId == questionare.Id).ToList();

                        foreach (var nonTech_db in list_nonTech_db)
                        {
                            if (list_nonTech_ss.Any(x => x.Id == nonTech_db.Id) == false)
                            {
                                //remove
                                db.C_Questionare_NonTechnician.Remove(nonTech_db);
                            }
                        }

                        foreach (var nonTech_ss in list_nonTech_ss)
                        {
                            var nonTech = list_nonTech_db.Where(x => x.Id == nonTech_ss.Id).FirstOrDefault();

                            if (nonTech == null)
                            {
                                //add
                                var new_nonTech = new C_Questionare_NonTechnician()
                                {
                                    Id = System.Guid.NewGuid().ToString(),
                                    QuestionareId = questionare.Id,
                                    Name = nonTech_ss.Name,
                                    Job = nonTech_ss.Job,
                                    Pay = nonTech_ss.Pay,
                                    AdjustViewPayroll = nonTech_ss.AdjustViewPayroll,
                                    CreateAt = DateTime.UtcNow,
                                };
                                db.C_Questionare_NonTechnician.Add(new_nonTech);
                            }
                            else
                            {
                                //update
                                nonTech.Name = nonTech_ss.Name;
                                nonTech.Job = nonTech_ss.Job;
                                nonTech.Pay = nonTech_ss.Pay;
                                nonTech.AdjustViewPayroll = nonTech_ss.AdjustViewPayroll;
                                db.Entry(nonTech).State = System.Data.Entity.EntityState.Modified;
                            }
                        }
                        #endregion

                        #region Update Technician
                        var list_Tech_ss = AddTechItem(int.Parse(Request["hd_techItem"]));
                        var list_Tech_db = db.C_Questionare_Technician.Where(x => x.QuestionareId == questionare.Id).ToList();

                        foreach (var Tech_db in list_Tech_db)
                        {
                            if (list_Tech_ss.Any(x => x.Id == Tech_db.Id) == false)
                            {
                                //remove
                                db.C_Questionare_Technician.Remove(Tech_db);
                            }
                        }

                        foreach (var Tech_ss in list_Tech_ss)
                        {
                            var Tech = list_Tech_db.Where(x => x.Id == Tech_ss.Id).FirstOrDefault();

                            if (Tech == null)
                            {
                                //add
                                var new_Tech = new C_Questionare_Technician()
                                {
                                    Id = System.Guid.NewGuid().ToString(),
                                    QuestionareId = questionare.Id,
                                    Name = Tech_ss.Name,
                                    NickName = Tech_ss.NickName,
                                    Commission = Tech_ss.Commission,
                                    PayrollSplitCheckOrCash = Tech_ss.PayrollSplitCheckOrCash,
                                    AddDiscounts = Tech_ss.AddDiscounts,
                                    AdjustPrices = Tech_ss.AdjustPrices,
                                    CreateAt = DateTime.UtcNow,
                                };
                                db.C_Questionare_Technician.Add(new_Tech);
                            }
                            else
                            {
                                //update
                                Tech.Name = Tech_ss.Name;
                                Tech.NickName = Tech_ss.NickName;
                                Tech.Commission = Tech_ss.Commission;
                                Tech.PayrollSplitCheckOrCash = Tech_ss.PayrollSplitCheckOrCash;
                                Tech.AddDiscounts = Tech_ss.AddDiscounts;
                                Tech.AdjustPrices = Tech_ss.AdjustPrices;
                                db.Entry(Tech).State = System.Data.Entity.EntityState.Modified;
                            }
                        }
                        #endregion

                        #region update merchant
                        var cus = db.C_Customer.Where(c => c.CustomerCode == questionare.CustomerCode).FirstOrDefault();
                        cus.BusinessName = questionare.SalonName;
                        cus.OwnerName = questionare.ContactName;
                        cus.OwnerMobile = questionare.ContactNumber;
                        cus.Email = questionare.OwnerEmail;

                        if (string.IsNullOrWhiteSpace(cus.BusinessAddressStreet))
                        {
                            cus.BusinessAddressStreet = questionare.SalonAddress1 + " " + questionare.SalonAddress2;
                        }
                        if (string.IsNullOrWhiteSpace(cus.BusinessCity))
                        {
                            cus.BusinessCity = questionare.SalonCity;
                        }
                        if (string.IsNullOrWhiteSpace(cus.BusinessState))
                        {
                            cus.BusinessState = questionare.SalonState;
                        }
                        if (string.IsNullOrWhiteSpace(cus.BusinessZipCode))
                        {
                            cus.BusinessZipCode = questionare.SalonZipcode;
                        }

                        cus.SalonEmail = questionare.SalonEmail;
                        cus.SalonPhone = questionare.SalonPhone;
                        cus.SalonTimeZone = questionare.SalonTimeZone;
                        cus.SalonTimeZone_Number = Ext.EnumParse<TIMEZONE_NUMBER>(questionare.SalonTimeZone).Text();
                        cus.SalonAddress1 = questionare.SalonAddress1;
                        cus.SalonAddress2 = questionare.SalonAddress2;
                        cus.SalonCity = questionare.SalonCity;
                        cus.SalonState = questionare.SalonState;
                        cus.SalonZipcode = questionare.SalonZipcode;
                        var countryDefault = "United States";
                        if (string.IsNullOrEmpty(cus.Country)) cus.Country = countryDefault;
                        if (string.IsNullOrEmpty(cus.BusinessCountry)) cus.BusinessCountry = countryDefault;

                        db.Entry(cus).State = System.Data.Entity.EntityState.Modified;

                        #endregion

                        //Save attach file
                        int filescount = Request.Files.Count;
                      await  UploadMultipleFilesAsync(questionare.CustomerId ?? 0, "C_Questionare", filescount, "/upload/other");

                        //Save image snapshot file
                        //int file_snap_count = int.Parse(Request["file-snap-count"].ToString());
                        //UploadMoreFiles(questionare.CustomerId ?? 0, "C_Questionare", file_snap_count, "/upload/img_snap");

                        //Send email cho nhan vien
                        await SendNotice_Questionnaire(questionare, true);

                        await db.SaveChangesAsync();
                        try
                        {
                            if (db.Store_Services.Any(s => s.StoreCode == cus.StoreCode && s.Active == 1) && !string.IsNullOrEmpty(cus.Version))
                            {
                                //PushStoreToMangoPos
                                var active_service_id = db.Store_Services.FirstOrDefault(s => s.StoreCode == cus.StoreCode && s.Active == 1)?.Id;
                                using (MerchantService service = new MerchantService(true))
                                {
                                    var rs = await service.ApproveAction(cus.StoreCode, active_service_id, true, "same-active");
                                }
                                    PaymentService.UpdateRecurringStatus();
                            }
                        }
                        catch(Exception ex)
                        {
                            // ignore
                            TempData["warning_msg"] = ex.Message;
                        }
                        // trans.Commit();
                        TempData["s"] = "Update questionnaire success";
                        return RedirectToAction("Result", new { id = questionare.Id });
                    }
                }
                catch (Exception ex)
                {
                    // trans.Dispose();
                    var cusId = long.Parse(Request["cusId"].ToString());
                    Session["error_msg"] = "There was an error. Please try again later or contact us.";
                    return RedirectToAction("Questionare", new { id = cusId, u = true });
                }
            }
        }

        public ActionResult Result(string id)
        {
            try
            {
                var _questionare = db.C_Questionare.Find(id);
                if (_questionare != null)
                {
                    var list_morefile = db.UploadMoreFiles.Where(f => f.TableId == _questionare.CustomerId && f.TableName.Equals("C_Questionare")).ToList();
                    ViewBag.MoreFiles = list_morefile.Where(x => x.FileName.Contains("img_snap") == false).ToList();
                    ViewBag.MoreFilesSnap = list_morefile.Where(x => x.FileName.Contains("img_snap") == true).ToList();

                    ViewBag.ListNonTechnician = db.C_Questionare_NonTechnician.Where(x => x.QuestionareId == _questionare.Id).OrderBy(x => x.CreateAt).ToList();
                    ViewBag.ListTechnician = db.C_Questionare_Technician.Where(x => x.QuestionareId == _questionare.Id).OrderBy(x => x.CreateAt).ToList();

                    if (TempData["s"] != null)
                    {
                        TempData["success"] = TempData["s"];
                    }
                    if (TempData["warning_msg"] != null)
                    {
                        TempData["warning"] = TempData["warning_msg"];
                    }
                    return View(_questionare);
                }
                else
                {
                    return RedirectToAction("err404");
                }
            }
            catch (Exception)
            {
                return RedirectToAction("err404");
            }
        }

        #region Add NonTechnician Item

        public List<QuestNonTechViewService> AddNonTechItem(int count_nonTech)
        {
            var list_nonTech = new List<QuestNonTechViewService>();
            for (int i = 1; i < count_nonTech; i++)
            {
                if (Request["manager_name" + i] != "")
                {
                    var nonTech = new QuestNonTechViewService()
                    {
                        Key = i,
                        Id = Request["manager_id" + i],
                        Name = Request["manager_name" + i],
                        Job = Request["manager_job" + i],
                        Pay = Request["manager_pay" + i],
                        AdjustViewPayroll = Request["manager_payroll" + i] == "1" ? true : false
                    };
                    list_nonTech.Add(nonTech);
                }
            }

            return list_nonTech;
        }

        #endregion

        #region Add Technician Item

        public List<QuestTechViewService> AddTechItem(int count_Tech)
        {
            var list_Tech = new List<QuestTechViewService>();
            for (int i = 1; i < count_Tech; i++)
            {
                if (Request["tech_name" + i] != "")
                {
                    var Tech = new QuestTechViewService()
                    {
                        Key = i,
                        Id = Request["tech_id" + i],
                        Name = Request["tech_name" + i],
                        NickName = Request["tech_nickname" + i],
                        Commission = Request["tech_comm" + i],
                        PayrollSplitCheckOrCash = Request["tech_pay" + i],
                        AddDiscounts = Request["tech_discout" + i] == "1" ? true : false,
                        AdjustPrices = Request["tech_price" + i] == "1" ? true : false
                    };
                    list_Tech.Add(Tech);
                }
            }

            return list_Tech;
        }

        #endregion

        #region Send mail notice when complete or update questionnaire

        public async Task<string> SendNotice_Questionnaire(C_Questionare questionare, bool update = false)
        {
            try
            {
                //Send email cho nhan vien
                //memberNumber_assigned: "0002, 0003, ..."


                var salesPerson = db.C_SalesLead.Where(x => x.CustomerCode == questionare.CustomerCode).FirstOrDefault();
                var memberNumber_Assigned = "";
                if (salesPerson != null) {
                    memberNumber_Assigned = salesPerson.MemberNumber?.Replace(" ", "");
                }
                var to_email = "";
                var to_fname = "";
                var to_email_bcc = "";
                var list_member = db.P_Member.Where(x => x.Active == true).ToList();
                if (!string.IsNullOrEmpty(memberNumber_Assigned))
                {
                    foreach (var memNumber in memberNumber_Assigned?.Split(new char[] { ',' }).ToList())
                    {
                        var mem = list_member.Where(x => x.MemberNumber == memNumber).FirstOrDefault();
                        if (mem != null)
                        {
                            to_email = string.IsNullOrEmpty(to_email) == true ? mem.PersonalEmail : (to_email + ";" + mem.PersonalEmail);
                            to_fname = string.IsNullOrEmpty(to_fname) == true ? mem.FirstName : (to_fname + ";" + mem.FirstName);
                        }
                    }
                     to_email_bcc = db.SystemConfigurations.FirstOrDefault()?.NotificationEmail;
                }
                else
                {
                    to_email = db.SystemConfigurations.FirstOrDefault()?.NotificationEmail;
                }
           

                if (string.IsNullOrEmpty(to_email) == false)
                {
                    var email_data = new SendGridEmailTemplateData.EmailNoticeQuestionareTemplate()
                    {
                        staff_name = "",
                        salon_name = questionare.SalonName,
                        update = update == true ? "updated" : "submitted",
                        url_question = Request.Url.Scheme + "://" + Request.Url.Authority + "/page/salon/Questionare/" + questionare.CustomerId.ToString() + "?n=" + CommonFunc.ConvertNonUnicodeURL(questionare.SalonName),
                        //url_ticket = Request.Url.Scheme + "://" + Request.Url.Authority + "/ticket/detail/" + ticket.Id.ToString()
                    };

                    ReadXML xml = new ReadXML(System.Web.Hosting.HostingEnvironment.MapPath("/App_Data/Config.xml"));
                    XmlNode node = xml.GetNode("/root/sendgrid_template/questionnaire_email_notice");
                    await _mailingService.SendBySendGridWithTemplate(to_email, to_fname, node["template_id"].InnerText, "", email_data, to_email_bcc);


                    //ticket feedback
                    //string fbTitle = "The questionnaire of " + questionare.SalonName + " have " + email_data.update;
                    //string fbConent = "Please click on the link below to review the questionnaire <br/><a target='_blank' href='" + email_data.url_question + "'>" + email_data.url_question + "</a>";
                }

                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        #endregion


        /// <summary>
        /// Handle update salon info
        /// </summary>
        /// <param name="id"></param>
        /// <param name="code"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public ActionResult Info(string id, string code, string target)
        {
            try
            {
                ViewBag.target = target;
                if (!string.IsNullOrEmpty(id))
                {
                    var cus = db.C_Customer.Where(c => c.StoreCode == id).FirstOrDefault();
                    if (cus?.MD5PassWord == code)
                    {
                        Session["ResetPassWord_md5_code"] = code;
                        ViewBag.LoginId = string.IsNullOrEmpty(cus.SalonEmail) ? cus.Email : cus.SalonEmail;
                    }
                    else
                    {
                        if (target == "forgot")
                        {
                            ViewBag.note = "Please use the link we have provided you!";
                        }
                        else
                        {
                            if (cus == null)
                            {
                                ViewBag.note = "Please use the link we have provided you!";
                            }
                            else
                            {
                                ViewBag.note = "Link expired! Please get new link to reset password!";
                            }
                        }
                        ViewBag.expired = true;
                    }
                }
                return View("info/PassWord.Change");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return RedirectToAction("err404");
            }

        }

        public async Task<JsonResult> ResetPassWord(string LoginId, string new_pass, string confirm_pass)
        {
            try
            {
                string md5_code = Session["ResetPassWord_md5_code"]?.ToString();
                var cus = db.C_Customer.Where(c => (c.SalonEmail == LoginId || c.Email == LoginId) && c.MD5PassWord == md5_code).FirstOrDefault();
                if (cus == null)
                {
                    throw new Exception("Your input incorrect! Please use the link we have provided you or input again!");
                }
                if (new_pass != confirm_pass)
                {
                    throw new Exception("Confirm password does not match!");
                }
                var md5_newpass = SecurityLibrary.Md5Encrypt(new_pass);
                if (md5_newpass == md5_code)
                {
                    throw new Exception("The new password must be different from the old password!");
                }
                // Todo : REMOVE PASS
                // cus.Password = null;
                cus.Password = new_pass;
                cus.MD5PassWord = md5_newpass;
                cus.RequirePassChange = "off";

                StoreProfile store = new MerchantService().GetStoreProfile(cus.StoreCode);
                if (store != null)
                {
                    store.Password = md5_newpass;
                    store.RequirePassChange = cus.RequirePassChange;
                    string partnerLink = new MerchantService().GetPartner(cus.CustomerCode).PosApiUrl;
                   // string url = AppConfig.Cfg.StoreChangeUrl(partnerLink);
                    string url = DomainConfig.NextGenApi + "/v1/RCPStore/StoreChange";
                    ClientRestAPI.CallRestApi(url, "", "", "post", store,SalonName: cus.BusinessName + " (#" + cus.StoreCode + ")");
                }
                Session.Remove("ResetPassWord_md5_code");
                db.Entry(cus).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Json(new object[] { true, "Change password completed" });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });
            }

        }
        //change password

        public async Task<JsonResult> MailForgotPassWord(string LoginId)
        {
            try
            {
                LoginId = LoginId.ToLower();
                var cus = db.C_Customer.Where(c => (
                    (c.MangoEmail != null && c.MangoEmail.Equals(LoginId, StringComparison.OrdinalIgnoreCase)) ||
                    (c.SalonEmail != null && c.SalonEmail.Equals(LoginId, StringComparison.OrdinalIgnoreCase)) ||
                    (c.Email != null && c.Email.Equals(LoginId, StringComparison.OrdinalIgnoreCase)))
                    && c.Active == 1 && string.IsNullOrEmpty(c.StoreCode) == false
                ).OrderByDescending(c => c.CreateAt).FirstOrDefault();
                if (cus == null)
                {
                    throw new Exception("We can't find info with your input : \"" + LoginId + "\" ");
                }
                if (string.IsNullOrEmpty(cus.MD5PassWord))
                {
                    var pass = cus.Password ?? SecurityLibrary.Md5Encrypt(DateTime.UtcNow.ToString("O")).Substring(0, 6);
                    cus.MD5PassWord = SecurityLibrary.Md5Encrypt(pass);
                    cus.Password = pass;
                    db.Entry(cus).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
                StoreProfile store = new MerchantService().GetStoreProfile(cus.StoreCode);
                ReadXML xml = new ReadXML(System.Web.Hosting.HostingEnvironment.MapPath("/App_Data/Config.xml"));
                XmlNode node = xml.GetNode("/root/sendgrid_template/salon_forgot_password");
                var email = string.IsNullOrEmpty(cus.SalonEmail) ? cus.Email : cus.SalonEmail;
                var email_data = new
                {
                    salon_name = cus.BusinessName,
                    reset_password_link = $"{Request.Url.Scheme}://{Request.Url.Authority}/Page/salon/info/{cus.StoreCode}?code={cus.MD5PassWord}&target=change"
                };

                string to = string.Join(";", cus.SalonEmail);
                string firstname = string.Join(";", cus.BusinessName);
                string cc = string.Join(";", cus.MangoEmail, store?.Email);
                string rs = await _mailingService.SendBySendGridWithTemplate(to, firstname, node["template_id"].InnerText, cc, email_data, "");
                //string rs = await SendEmail.SendBySendGridWithTemplate(store?.Email ?? email, store?.StoreName ?? store?.Email ?? email ?? string.Empty, node["template_id"].InnerText, "", email_data, "");
                if (string.IsNullOrEmpty(rs))
                {
                    return Json(new object[] { true,
                        "An email has been sent to you with instructions on how to change your password. Please check your inbox." +
                        " <br/> If it doesn’t appear within a few minutes, check your spam folder."
                    });
                }
                else
                {
                    throw new Exception("Send email to " + store?.Email ?? email + " fail.");
                }

            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });
            }
        }
    }
}
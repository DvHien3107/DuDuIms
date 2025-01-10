using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Models.CustomizeModel;
using EnrichcousBackOffice.AppLB;
using System.IO;
using EnrichcousBackOffice.AppLB.FilePDF;
using EnrichcousBackOffice.Utils.IEnums;
using Inner.Libs.Helpful;
using EnrichcousBackOffice.Areas.Page.Models.Customize;

namespace EnrichcousBackOffice.Controllers
{
    [Authorize]
    public class MerchantFormManageController : Controller
    {
        private Dictionary<string, bool> access = AppLB.Authority.GetAccessAuthority();
        WebDataModel db = new WebDataModel();
        private P_Member cMem = AppLB.Authority.GetCurrentMember();

        // GET: MerchantFormManage
        public ActionResult Index()
        {
            //add to view history top button
            UserContent.TabHistory = "Merchant Form|" + Request.Url.AbsolutePath;

            if (access.Any(k => k.Key.Equals("merchantforms_manage")) != true || access["merchantforms_manage"] != true)
            {
                TempData["e"] = "You are not permission to access.";
                return RedirectToAction("forbidden", "home");
            }

            ViewBag.canLoadMore = db.O_MerchantForm.Count() > 5;
            var historyPdf = db.O_MerchantForm.OrderByDescending(f => f.UpdateAt ?? f.CreateAt).Take(5).ToList();
            return View(historyPdf);
        }

        public ActionResult Load(string Searchtext, int Merchant_type, int page = 0)
        {
            int mer_on_page = 6;
            string store_in_house = MerchantType.STORE_IN_HOUSE.Code<string>();
            var qr = db.C_Customer.Where(c=> !string.IsNullOrEmpty(c.WordDetermine) && c.Type != store_in_house);
            var merchants = db.C_MerchantSubscribe.Where(p => p.Status.Equals("active") || p.Status.Equals("closed")).Select(o => o.CustomerCode).Distinct().ToList();
            switch (Merchant_type)
            {
                case 1://lead
                    var cancel_status = InvoiceStatus.Canceled.ToString();
                    var has_invoice = db.O_Orders.Where(o => o.Status != cancel_status).Select(o => o.CustomerCode).Distinct().ToList();
                    qr = qr.Where(c => has_invoice.Contains(c.CustomerCode) && !merchants.Contains(c.CustomerCode));
                    break;
                case 2://merchant
                    qr = qr.Where(c => merchants.Contains(c.CustomerCode));
                    break;
            }

            var rs = qr.AsEnumerable().Where(c => 
            CommonFunc.SearchName(c.BusinessName, Searchtext) 
            || CommonFunc.SearchName(c.SalonPhone, Searchtext) || CommonFunc.SearchName(c.SalonEmail, Searchtext)
            || CommonFunc.SearchName(c.BusinessPhone, Searchtext) || CommonFunc.SearchName(c.BusinessEmail, Searchtext)
            || CommonFunc.SearchName(c.StoreCode, Searchtext));
            //var a = qr.ToList();
            ViewBag.countPages = (rs.Count() - 1) / mer_on_page + 1;
            ViewBag.Page = page;
            var merchant_list = rs.Skip(mer_on_page * page).Take(mer_on_page).Select(c =>
            new MerchantFormView { Id = c.Id, WordDetermine = c.WordDetermine, Code = c.CustomerCode, Name = (string.IsNullOrEmpty(c.BusinessName) ? c.OwnerName : c.BusinessName), List_pdf = db.O_MerchantForm.Where(m => m.MerchantCode == c.CustomerCode).ToList() }).OrderByDescending(c => c.Id);

            ViewBag.ListOrder = db.O_Orders.ToList();
            return PartialView("_PartialMerchantFormList", merchant_list);
        }
        //public void Search(IQueryable<Object> qr, string col_name, string 1%2%12)
        //{
        //    Queryable a = new Queryable("ádjhasvgduiwakgduahds"); 
        //}
        public ActionResult Load_history(int skip)
        {
            bool canLoadMore = db.O_MerchantForm.Count() > (skip + 5);
            var historyPdf = db.O_MerchantForm.OrderByDescending(f => f.UpdateAt ?? f.CreateAt).Skip(skip).Take(5).ToList().Select(f => new
            {
                f.Id,
                f.MerchantName,
                f.MerchantCode,
                f.OrderId,
                CreateAt = f.CreateAt?.ToString("dd MMM yyyy - HH:mm"),
                CreateBy = f.CreateBy,
                UpdateAt = f.UpdateAt?.ToString("dd MMM yyyy - HH:mm"),
                SendAt = f.SendAt?.ToString("dd MMM yyyy - HH:mm"),
                SendByAgent = f.SendByAgent?.Split('|')[1],
                f.TemplateName,
                f.Status,
                f.PDF_URL,
                f.TerminalId,
                f.Signed
            });
            return Json(new object[] { true, historyPdf, canLoadMore });
        }

        public JsonResult LoadNewHistory()
        {
            try
            {
                bool canLoadMore = db.O_MerchantForm.Count() > 5;
                var list_NewPDF = new List<O_MerchantForm>();
                var NewHistoryPdf = db.O_MerchantForm.OrderByDescending(f => f.UpdateAt ?? f.CreateAt).Take(5).ToList().Select(f => new
                {
                    f.Id,
                    f.MerchantName,
                    f.MerchantCode,
                    f.OrderId,
                    CreateAt = f.CreateAt?.ToString("dd MMM yyyy - HH:mm"),
                    CreateBy = f.CreateBy,
                    UpdateAt = f.UpdateAt?.ToString("dd MMM yyyy - HH:mm"),
                    SendAt = f.SendAt?.ToString("dd MMM yyyy - HH:mm"),
                    SendByAgent = f.SendByAgent?.Split('|')[1],
                    f.TemplateName,
                    f.Status,
                    f.PDF_URL,
                    f.TerminalId,
                    f.Signed
                });

                return Json(new object[] { true, NewHistoryPdf, canLoadMore });
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, "Error: " + ex.Message });
            }
        }

        public JsonResult GetListTemplate()
        {
            try
            {
                DirectoryInfo d = new DirectoryInfo(Server.MapPath("~/upload/merchant/template"));//Assuming Test is your Folder
                FileInfo[] Files = d.GetFiles("*.pdf"); //Getting PDF files
                List<string> ListFileName = new List<string>();
                foreach (FileInfo file in Files)
                {
                    if (file.Name != "DEJAVOO_Z11_DATA_SHEET.pdf")
                    {
                        ListFileName.Add(file.Name.Replace(".pdf", ""));
                    }
                }
                return Json(new object[] { true, ListFileName });
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, ex.Message });
            }
        }

        public JsonResult GetListOrder(string merchantCode, string templateName)
        {
            try
            {
                if (templateName == "One-Time Payment ACH")
                {
                    var list_order = (from o in db.O_Orders
                                      join op in db.Order_Products
                                      on o.OrdersCode equals op.OrderCode
                                      where o.CustomerCode == merchantCode && o.InvoiceNumber > 0
                                      && o.IsDelete != true && o.Cancel != true
                                      orderby o.CreatedAt descending
                                      select new { o.Id, o.OrdersCode }).ToList();

                    var list_order_onetime = list_order;

                    list_order = (from o in db.O_Orders
                                  join os in db.Order_Subcription
                                  on o.OrdersCode equals os.OrderCode
                                  where o.CustomerCode == merchantCode && o.InvoiceNumber > 0
                                  && o.IsDelete != true && o.Cancel != true && string.IsNullOrEmpty(os.Period)
                                  orderby o.CreatedAt descending
                                  select new { o.Id, o.OrdersCode }).ToList();
                    if (list_order.Count() > 0)
                    {
                        list_order_onetime.AddRange(list_order);
                    }

                    return Json(new object[] { true, list_order_onetime.Distinct() });
                }
                else if (templateName == "Recurring Payment ACH")
                {
                    var list_order_recurring = (from o in db.O_Orders
                                                join os in db.Order_Subcription
                                                on o.OrdersCode equals os.OrderCode
                                                where o.CustomerCode == merchantCode && o.InvoiceNumber > 0
                                                && o.IsDelete != true && o.Cancel != true
                                                && os.IsAddon != true && !string.IsNullOrEmpty(os.Period)
                                                orderby o.CreatedAt descending
                                                select new { o.Id, o.OrdersCode }).ToList();

                    return Json(new object[] { true, list_order_recurring });
                }
                else if (templateName == "DEJAVOO Z11 DATA SHEET")
                {
                    var list_order_recurring = (from o in db.O_Orders
                                                join op in db.Order_Products on o.OrdersCode equals op.OrderCode
                                                join p in db.O_Product on op.ProductCode equals p.Code
                                                where o.CustomerCode == merchantCode && o.InvoiceNumber > 0
                                                && o.IsDelete != true && o.Cancel != true
                                                && p.ProductLineCode == "terminal"
                                                orderby o.CreatedAt descending
                                                select new { o.Id, o.OrdersCode }).ToList();

                    return Json(new object[] { true, list_order_recurring });
                }

                return Json(new object[] { true, new List<O_Orders>() });
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, ex.Message });
            }
        }

        public JsonResult GetPeriod(string OrderCode)
        {
            try
            {
                var list_period = (from os in db.Order_Subcription
                                   where os.OrderCode == OrderCode
                                   && os.Actived != false && os.IsAddon != true
                                   orderby os.Period
                                   group os by os.Period into gr
                                   select gr.Key).ToList();

                return Json(new object[] { true, list_period });
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, ex.Message });
            }
        }

        public JsonResult GetListTerminal(string orderCode)
        {
            try
            {
                var list_order_product = db.Order_Products.Where(x => x.OrderCode == orderCode && x.InvNumbers != null).ToList();
                var list_InvNumbers = new List<string>();
                foreach (var op in list_order_product)
                {
                    list_InvNumbers.AddRange(op.InvNumbers.Split(','));
                }
                var list_terminal = (from d in db.O_Device.AsEnumerable()
                                     join p in db.O_Product on d.ProductCode equals p.Code
                                     where p.ProductLineCode == "terminal" && list_InvNumbers.Contains(d.InvNumber)
                                     select d).ToList();


                return Json(new object[] { true, list_terminal });
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, ex.Message });
            }
        }

        #region Add File & Create File
        public ActionResult AddFile(string templateName, string customerCode, string orderCode, long? terminalId)
        {
            try
            {
                if (templateName == "DEJAVOO Z11 DATA SHEET")
                {
                    Dejavo_Datasheet dejavoo = DejavooPDF.GetDejavooInfo(customerCode, orderCode, terminalId);
                    return PartialView("_DejavooPopupPartial", dejavoo);
                }
            }
            catch (Exception ex)
            {
                TempData["e"] = ex.Message;
            }

            return RedirectToAction("index");
        }
        public ActionResult UpdateFile(long merchant_form_id)
        {

            try
            {
                var mf = db.O_MerchantForm.Find(merchant_form_id);
                if (mf.TemplateName == "DEJAVOO Z11 DATA SHEET")
                {
                    var order_code = db.O_Orders.Find(mf.OrderId).OrdersCode;
                    Dejavo_Datasheet dejavoo = DejavooPDF.GetDejavooInfo(mf.MerchantCode, order_code, mf.TerminalId);
                    return PartialView("_DejavooPopupPartial", dejavoo);
                }
            }
            catch (Exception ex)
            {
                TempData["e"] = ex.Message;
            }

            return RedirectToAction("index");
        }

        /// <summary>
        /// Create dejavoo file pdf
        /// </summary>
        /// <param name="dejavoModel"></param>
        /// <returns></returns>
        public JsonResult CreateDejavoo(Dejavo_Datasheet dejavoModel)
        {
            try
            {
                var create_complete = DejavooPDF.CreateDejavooPDF(dejavoModel);

                if (string.IsNullOrEmpty(create_complete) == false)
                {
                    return Json(new object[] { true, "Create PDF complete.", create_complete });
                }
                else
                {
                    throw new Exception("Create PDF failure.");
                }
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, "Error: " + ex.Message });
            }
        }

        /// <summary>
        /// Create bank change form pdf
        /// </summary>
        /// <param name="customerCode"></param>
        /// <returns></returns>
        public JsonResult CreateBankChangeForm(string customerCode)
        {
            try
            {
                var create_complete = BankChangeFormPDF.CreateBankChangeForm(customerCode);
                if (string.IsNullOrEmpty(create_complete) == false)
                {
                    return Json(new object[] { true, "Create PDF complete.", create_complete.Split('|')[0], create_complete.Split('|')[1], create_complete.Split('|')[2] });
                }
                else
                {
                    throw new Exception("Create PDF failure.");
                }
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, "Error: " + ex.Message });
            }
        }

        /// <summary>
        /// Create one time payment pdf
        /// </summary>
        /// <param name="customerCode"></param>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public JsonResult CreateOneTimePayment(string customerCode, string orderCode)
        {
            try
            {
                var create_complete = OneTimePaymentPDF.CreateOneTimePayment(customerCode, orderCode);
                if (string.IsNullOrEmpty(create_complete) == false)
                {
                    return Json(new object[] { true, "Create PDF complete.", create_complete.Split('|')[0], create_complete.Split('|')[1], create_complete.Split('|')[2] });
                }
                else
                {
                    throw new Exception("Create PDF failure.");
                }
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, "Error: " + ex.Message });
            }
        }
        #endregion

        #region Send pdf file
        public JsonResult OpenFileAfterCheck(long? id)
        {
            try
            {
                var file = db.O_MerchantForm.Find(id);
                if (file != null)
                {
                    string usign_url = "";
                    if (file.Signed == true)
                    {
                        usign_url = file.PDF_URL.Insert(file.PDF_URL.LastIndexOf('.'), "_signed");
                    }

                    return Json(new object[] { true, file.Id, file.TemplateName, file.PDF_URL, file.TerminalId, usign_url });
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, ex.Message });
            }
        }

        public JsonResult SendPDF(long? fileId)
        {
            string errMsg = string.Empty;
            try
            {
                var merchant_form = db.O_MerchantForm.Where(x => x.Id == fileId).FirstOrDefault();
                if (merchant_form != null)
                {
                    #region Get access token
                    string accessToken = AppLB.DocuSign.DocuSignRestAPI.GetToken(out errMsg);
                    if (accessToken == "-1")
                    {
                        //TH chua co Access Token
                        throw new Exception("Login for get access token");
                    }
                    else if (string.IsNullOrEmpty(accessToken) == true)
                    {
                        throw new Exception(errMsg);
                    }
                    #endregion


                    #region Send Document
                    var merchant = db.C_Customer.Where(c => c.CustomerCode == merchant_form.MerchantCode).FirstOrDefault();
                    string signer_name = string.IsNullOrEmpty(merchant.OwnerName) == false ? merchant.OwnerName : merchant.CompanyName;
                    string signer_email = string.IsNullOrEmpty(merchant.Email) == false ? merchant.Email : merchant.BusinessEmail;
                    if (string.IsNullOrWhiteSpace(signer_email))
                    {
                        throw new Exception("Merchant email is invalid. Please update your merchant profile");
                    }

                    string file_path = merchant_form.PDF_URL;
                    string envelopeId = AppLB.DocuSign.DocuSignRestAPI.SendDocument(accessToken, file_path, signer_name, signer_email, out errMsg);
                    #endregion

                    if (string.IsNullOrEmpty(envelopeId))
                    {
                        throw new Exception(errMsg);
                    }

                    //save AgreementId vao db
                    merchant_form.AgreementId = envelopeId;
                    merchant_form.UpdateBy = cMem.FullName;
                    merchant_form.UpdateAt = DateTime.UtcNow;
                    merchant_form.Send = true;
                    merchant_form.Status = "sent";
                    merchant_form.SendAt = DateTime.UtcNow;
                    merchant_form.SendByAgent = cMem.MemberNumber + "|" + cMem.FullName + "|" + cMem.PersonalEmail;
                    db.Entry(merchant_form).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    return Json(new object[] { true, "Send file success!" });
                }
                else
                {
                    throw new Exception("File not found!");
                }
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, ex.Message });
            }
        }
        #endregion
        public JsonResult GetFileSigned_Url(long? id, string url)
        {
            string errMsg = string.Empty;
            try
            {
                var file = db.O_MerchantForm.Find(id);
                if (file != null)
                {
                    bool file_exist = System.IO.File.Exists(Server.MapPath(url));
                    if (file.Signed == true && file_exist == false)
                    {
                        #region Get access token
                        string accessToken = AppLB.DocuSign.DocuSignRestAPI.GetToken(out errMsg);
                        if (accessToken == "-1")
                        {
                            //TH chua co Access Token
                            throw new Exception("Login for get access token");
                        }
                        else if (string.IsNullOrEmpty(accessToken) == true)
                        {
                            throw new Exception(errMsg);
                        }
                        #endregion

                        var docStream = AppLB.DocuSign.DocuSignRestAPI.DownloadDocument_PDF(accessToken, file.AgreementId, out errMsg);
                        if (docStream != null)
                        {
                            string Path = Server.MapPath(url);
                            using (var stream = System.IO.File.Create(Path))
                                docStream.CopyTo(stream);
                        }
                        else
                        {
                            throw new Exception(errMsg);
                        }
                    }
                }

                return Json(new object[] { true, url });
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, ex.Message });
            }
        }

    }

    internal class Pdf_info
    {
        public string MerchantName { get; set; }
        public string CreateAt { get; set; }
        public string TemplateName { get; set; }
        public string Status { get; set; }
    }
}
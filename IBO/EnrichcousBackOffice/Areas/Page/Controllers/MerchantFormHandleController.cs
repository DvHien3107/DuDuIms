using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EnrichcousBackOffice.Models;
using SelectPdf;
using EnrichcousBackOffice.Areas.Page.Models.Customize;
using System.Diagnostics;
using EnrichcousBackOffice.Controllers;
using System.IO;
using iTextSharp.text.pdf;
using EnrichcousBackOffice.Models.CustomizeModel;
using EnrichcousBackOffice.ViewModel;
using Newtonsoft.Json;
using NPOI.Util;
using Inner.Libs.Helpful;
using EnrichcousBackOffice.AppLB;

namespace EnrichcousBackOffice.Areas.Page.Controllers
{
    [Authorize]
    public class MerchantFormHandleController : UploadController
    {
        private WebDataModel db = new WebDataModel();
        private P_Member cMem = AppLB.Authority.GetCurrentMember();
        const string BankChangeFormData = "BankChangeFormData";
        const string PaymentFormData = "PaymentFormData";
        const string RefundFormData = "RefundFormData";
        const string NameChangeFormData = "NameChangeFormData";
        // GET: Page/MerchantFormHandle
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult _changeRequestForm()
        {
            return View();
        }
        /// <summary>
        /// Layout Form Page
        /// </summary>
        /// <param name="t">template name</param>
        /// <param name="mc">merchant code</param>
        /// <param name="oc">order code</param>
        /// <param name="u">update ["true" or "false"]</param>
        /// <param name="p">order subscription Period</param>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult Form(string t, string mc, string oc, string u = "false", string p = "")
        {
            oc = oc == "null" ? "" : oc;
            string view = string.Empty;
            try
            {
                long? orderId = db.O_Orders.Where(o => o.OrdersCode == oc).FirstOrDefault()?.Id ?? (-1);
                var CompanyInfo = db.SystemConfigurations.FirstOrDefault();
                var MerchantInfo = db.C_Customer.Where(m => m.CustomerCode == mc).FirstOrDefault();
                var MID = db.C_MerchantSubscribe.Where(m => m.CustomerCode == mc && !m.Status.Equals("inactive")).OrderByDescending(m => m.Status.Equals("active")).FirstOrDefault()?.MerchantID ?? "";


                var merchant_form = (from mf in db.O_MerchantForm.AsEnumerable()
                                     where mf.MerchantCode == mc
                                     && mf.TemplateName.Equals(t, StringComparison.OrdinalIgnoreCase)
                                     && (orderId == -1 || mf.OrderId == orderId)
                                     select mf).FirstOrDefault();

                if (t.Equals("BANK CHANGE FORM", StringComparison.OrdinalIgnoreCase))
                {
                    #region Bank Change Form
                    string address = MerchantInfo.BusinessAddressStreet + ", " + MerchantInfo.BusinessCity + ", " + MerchantInfo.BusinessState + ", " + MerchantInfo.BusinessZipCode + ", " + MerchantInfo.BusinessCountry;

                    var BCF_Data = new FormModel.BankChangeForm_Data();
                    BCF_Data.OwnerName = MerchantInfo.OwnerName;
                    BCF_Data.BusinessName = MerchantInfo.LegalName ?? MerchantInfo.BusinessName;
                    BCF_Data.Date = MerchantInfo.BusinessStartDate?.ToString("MM/dd/yyyy");
                    BCF_Data.MID = MID;
                    BCF_Data.PhoneNumber = AppLB.CommonFunc.getStringValid(MerchantInfo.BusinessPhone, MerchantInfo.SalonPhone, MerchantInfo.CellPhone);
                    Session[BankChangeFormData] = BCF_Data;
                    #endregion

                    view = CommonFunc.RenderRazorViewToString("_BankChangeForm",null, this);
                }
                else if (t.Equals("CHANGE REQUEST FORM", StringComparison.OrdinalIgnoreCase))
                {
                    #region CHANGE REQUEST FORM
                    var pdf_data = Session["data_to_pdf"] as FormModel.NameChangeForm_Data;
                    if (pdf_data != null)
                    {
                        Session[NameChangeFormData] = pdf_data;
                        Session.Remove("data_to_pdf");
                    }
                    else
                    {
                        string address = MerchantInfo.BusinessAddressStreet + ", " + MerchantInfo.BusinessCity + ", " + MerchantInfo.BusinessState + ", " + MerchantInfo.BusinessZipCode + ", " + MerchantInfo.BusinessCountry;

                        var NCF_Data = new FormModel.NameChangeForm_Data();
                        NCF_Data.Date = DateTime.Now.ToString("MM/dd/yyyy");
                        NCF_Data.MID = MID;
                        NCF_Data.OwnerName = MerchantInfo.OwnerName;
                        NCF_Data.BusinessName = MerchantInfo.BusinessName;
                        NCF_Data.LegalName = MerchantInfo.LegalName;
                        Session[NameChangeFormData] = NCF_Data;
                        pdf_data = NCF_Data;
                    }
                    #endregion
                    view = CommonFunc.RenderRazorViewToString("_ChangeRequestForm", pdf_data, this);
                }
                else if (t.Equals("One-Time Payment ACH", StringComparison.OrdinalIgnoreCase) || t.Equals("Recurring Payment ACH", StringComparison.OrdinalIgnoreCase))
                {
                    #region One-Time Payment ACH & Recurring Payment ACH
                    var order = db.O_Orders.Where(o => o.Id == orderId).FirstOrDefault();
                    ViewBag.OrderInfo = order;
                    var list_order_service = db.Order_Subcription.Where(s => s.OrderCode == order.OrdersCode && s.Actived == true).ToList();
                    ViewBag.ListOrderService = list_order_service;

                    if (t.Equals("One-Time Payment ACH", StringComparison.OrdinalIgnoreCase))
                    {
                        ViewBag.ListOrderProduct = db.Order_Products.Where(_p => _p.OrderCode == order.OrdersCode).ToList();
                    }

                    if (t.Equals("Recurring Payment ACH", StringComparison.OrdinalIgnoreCase))
                    {
                        ViewBag.ListOrderService = list_order_service.Where(os => os.Period == p).ToList();

                        var list_license_pro = db.License_Product.ToList();
                        foreach (var ser in ViewBag.ListOrderService as List<Order_Subcription>)
                        {
                            var license_product = list_license_pro.Where(x => x.Id == ser.ProductId).FirstOrDefault();
                            if (license_product?.Type == "license")
                            {
                                ViewBag.MainServiceName = license_product?.Name;
                            }
                        }
                    }

                    var Payment_Data = new FormModel.PaymentForm_Data();
                    Payment_Data.OwnerName = MerchantInfo.OwnerName;
                    Payment_Data.MerchantCode = MerchantInfo.CustomerCode;
                    Payment_Data.GrandTotal = order?.GrandTotal;
                    Payment_Data.Day = order.InvoiceDate?.ToString("dd");
                    Payment_Data.Month = order.InvoiceDate?.ToString("MM");
                    Payment_Data.Year = order.InvoiceDate?.ToString("yyyy");
                    Payment_Data.InvoiceNumber = order.InvoiceNumber;
                    Payment_Data.BusinessStreetAddress = MerchantInfo.BusinessAddressStreet;
                    Payment_Data.BusinessCity = MerchantInfo.BusinessCity;
                    Payment_Data.BusinessState = MerchantInfo.BusinessState;
                    Payment_Data.BusinessZip = MerchantInfo.BusinessZipCode;
                    Payment_Data.Phone = string.IsNullOrEmpty(MerchantInfo.CellPhone) == true ? MerchantInfo.BusinessPhone : MerchantInfo.CellPhone;
                    Payment_Data.Email = string.IsNullOrEmpty(MerchantInfo.Email) == true ? MerchantInfo.BusinessEmail : MerchantInfo.Email;
                    Payment_Data.Routing = MerchantInfo.DepositRoutingNumber;
                    Payment_Data.AccountNumber = MerchantInfo.DepositAccountNumber;
                    Payment_Data.BankName = MerchantInfo.DepositBankName;
                    Payment_Data.MID = MID ?? "";
                    Payment_Data.BusinessName = MerchantInfo.LegalName;
                    Session[PaymentFormData] = Payment_Data;
                    #endregion
                    ViewBag.Update = u;
                    if (t.Equals("Recurring Payment ACH", StringComparison.OrdinalIgnoreCase))
                    {
                        view = CommonFunc.RenderRazorViewToString("_RecurringPaymentACH_Partial", CompanyInfo, this);
                    }
                    else
                    {
                        ViewBag.Flag = "Invoices";
                        ViewBag.invoice_title = "ONE-TIME INVOICE";
                        ViewBag.Customer = db.C_Customer.Where(c => c.CustomerCode == order.CustomerCode).FirstOrDefault() ?? new C_Customer();
                        ViewBag.CompanyInfo = db.SystemConfigurations.FirstOrDefault();
                        var _listOrderProduct = db.Order_Products.Where(_p => _p.OrderCode == order.OrdersCode)
                            .Select(_p => new Order_Products_view
                            {
                                order_model = _p,
                                device_Infos = db.O_Device.Where(d => _p.InvNumbers.Contains(d.InvNumber))
                                .Select(d => new Order_Products_view.device_info { inv_number = d.InvNumber, serial_number = d.SerialNumber }).ToList(),
                            }).ToList();
                        ViewBag.ListOrderPackage = (from _p in _listOrderProduct
                                                    group _p by _p.order_model.BundleId into g_p
                                                    join b in db.I_Bundle on g_p.Key equals b.Id into _b
                                                    from b in _b.DefaultIfEmpty()
                                                    select new Order_Package_view
                                                    {
                                                        Package = b,
                                                        Products = g_p.ToList(),
                                                    }).ToList();
                        if (_listOrderProduct != null)
                        {
                            var tracking_number = db.Order_Carrier.Where(o => o.OrderCode == order.OrdersCode).FirstOrDefault();
                            ViewBag.tracking = tracking_number;
                        }

                        ViewBag.ListOrderSubcription = db.Order_Subcription.Where(s => s.OrderCode == order.OrdersCode && s.Actived == true)
                            .Select(os => new VmOrderService
                            {
                                Product_Code = os.Product_Code,
                                Period = os.Period,
                                ProductName = os.ProductName,
                                StartDate = os.StartDate,
                                EndDate = os.EndDate,
                                Price = os.Price,
                                Quantity = os.Quantity ?? 1,
                                Discount = os.Discount ?? 0,
                                DiscountPercent = os.DiscountPercent ?? 0,
                                Amount = os.Period == "MONTHLY" ? (os.Price ?? 0) - (os.Discount ?? 0) : ((os.Price ?? 0) * (os.Quantity ?? 1)) - (os.Discount ?? 0),
                                IsAddon = os.IsAddon
                            }).ToList();
                        view = CommonFunc.RenderRazorViewToString("_OneTimePaymentACH_Partial", CompanyInfo, this);
                    }
                }
                else if (t.Equals("Refund", StringComparison.OrdinalIgnoreCase))
                {
                    #region Refund
                    var order = db.O_Orders.Where(o => o.Id == orderId).FirstOrDefault();
                    ViewBag.OrderInfo = order;
                    var refund = db.O_Order_Refund.Where(x => x.OrderCode == order.OrdersCode).FirstOrDefault();

                    var Refund_Data = new FormModel.RefundForm_Data();
                    Refund_Data.OwnerName = MerchantInfo.OwnerName;
                    Refund_Data.MerchantCode = MerchantInfo.CustomerCode;
                    Refund_Data.OrderCode = order.OrdersCode;
                    Refund_Data.BusinessStreetAddress = MerchantInfo.BusinessAddressStreet;
                    Refund_Data.BusinessCity = MerchantInfo.BusinessCity;
                    Refund_Data.BusinessState = MerchantInfo.BusinessState;
                    Refund_Data.BusinessZip = MerchantInfo.BusinessZipCode;
                    Refund_Data.MID = MID ?? "";
                    Refund_Data.BusinessName = MerchantInfo.LegalName;
                    Refund_Data.RefundCode = refund?.Code;
                    Refund_Data.RefundAmount = refund?.RefundAmout;
                    Refund_Data.Reason = refund?.Reason;
                    Refund_Data.Subject = refund?.Subject;
                    Refund_Data.Content = refund?.Content;
                    Session[RefundFormData] = Refund_Data;
                    #endregion

                    ViewBag.Update = u;
                    view = CommonFunc.RenderRazorViewToString("_RefundPartial", CompanyInfo, this);
                }
                else
                {
                    throw new Exception("Template not found.");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                view = CommonFunc.RenderRazorViewToString("_BankChangeForm", null, this);
            }
            ViewBag.TemplateName = t;
            return View(model: view);
        }

        ///// <summary>
        ///// Load partial form by template name
        ///// </summary>
        ///// <param name="templateName"></param>
        ///// <param name="merchantCode"></param>
        ///// <param name="orderCode"></param>
        ///// <param name="update">["true" or "false"]</param>
        ///// <returns></returns>
        //[AllowAnonymous]
        //public ActionResult LoadPartial(string templateName, string merchantCode, string orderCode, string update, string period)
        //{
            
        //}

        /// <summary>
        /// Get Url of exist File when View OR Url Form Page when Create 
        /// </summary>
        /// <param name="Key">["view": when view file] OR ["" or "null": when create file]</param>
        /// <param name="templateName"></param>
        /// <param name="merchantCode"></param>
        /// <param name="orderCode"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public JsonResult GetFileUrl(string Key, string templateName, string merchantCode, string orderCode, string period, string pdf_nuvei_type, long? merchantFormId)
        {
            try
            {
                orderCode = orderCode == "null" ? "" : orderCode;
                long? orderId = db.O_Orders.Where(o => o.OrdersCode == orderCode).FirstOrDefault()?.Id ?? (-1);
                string errMsg = string.Empty;
                string file_url = "";
                string file_signed_url = "";

                var merchant_form = db.O_MerchantForm.Find(merchantFormId);

                if (merchant_form != null && Key == "view")
                {
                    //view file
                    bool file_exist = System.IO.File.Exists(Server.MapPath(merchant_form.PDF_URL));
                    if (string.IsNullOrEmpty(merchant_form.PDF_URL) == true || file_exist == false)
                    {
                        throw new Exception("File url not found", new Exception(merchant_form.PDF_URL));
                    }

                    if (templateName == "Nuvei")
                    {
                        string pdf = Server.MapPath(merchant_form.PDF_URL);
                        string flatfile_path = merchant_form.PDF_URL.Insert(merchant_form.PDF_URL.LastIndexOf('.'), "_flat");
                        if (System.IO.File.Exists(Server.MapPath(flatfile_path)))
                        {
                            System.IO.File.Delete(Server.MapPath(flatfile_path));
                        }
                        string flatFile = Server.MapPath(flatfile_path);
                        PdfReader pdfReader = new PdfReader(pdf);
                        using (PdfStamper pdfStamper = new PdfStamper(pdfReader, new FileStream(flatFile, FileMode.Create)))
                        {
                            AcroFields pdfFormFields = pdfStamper.AcroFields;
                            pdfStamper.FormFlattening = true;
                        }
                        file_url = flatfile_path;
                        pdfReader.Close();
                    }
                    else
                    {
                        file_url = merchant_form.PDF_URL;
                    }
                    if (merchant_form.Signed == true)
                    {
                        file_signed_url = merchant_form.PDF_URL?.Replace(".pdf", "_signed.pdf");
                        //kiểm tra file_signed có tồn tại hay không
                        file_exist = System.IO.File.Exists(Server.MapPath(file_signed_url));

                        //nếu chưa có thì download về (dùng DocuSign)
                        if (file_exist == false)
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

                            #region Download document
                            var docStream = AppLB.DocuSign.DocuSignRestAPI.DownloadDocument_PDF(accessToken, merchant_form.AgreementId, out errMsg);
                            if (docStream != null)
                            {
                                string Path = Server.MapPath(file_signed_url);
                                using (var stream = System.IO.File.Create(Path))
                                    docStream.CopyTo(stream);
                            }
                            else
                            {
                                throw new Exception(errMsg);
                            }
                            #endregion
                        }
                    }
                    if (!string.IsNullOrEmpty(pdf_nuvei_type))
                    {
                        templateName = pdf_nuvei_type;
                    }
                }
                else
                {
                    if (templateName == "RequestForm")
                    {
                        //file_url = "/Page/form/?mc=" + merchantCode + "&mn=" + cMem.MemberNumber;
                        return Json(new object[] { false });
                    }
                    else if (templateName.Contains("Nuvei"))
                    {
                        if (System.IO.File.Exists(Server.MapPath("/upload/merchant/Merchant_" + merchantCode + "/" + pdf_nuvei_type + "_" + merchantCode + ".pdf")))
                        {
                            System.IO.File.Delete(Server.MapPath("/upload/merchant/Merchant_" + merchantCode + "/" + pdf_nuvei_type + "_" + merchantCode + ".pdf"));
                        }

                        UploadAttachFile("/upload/merchant/Merchant_" + merchantCode + "/", "file_attachment", "", pdf_nuvei_type + "_" + merchantCode + ".pdf", out file_url);
                        var mf = db.O_MerchantForm.Where(m => m.MerchantCode == merchantCode && m.TemplateName.Contains(pdf_nuvei_type)).FirstOrDefault();
                        if (mf == null)
                        {
                            var merchant = db.C_Customer.Where(c => c.CustomerCode == merchantCode).FirstOrDefault();
                            var new_mf = new O_MerchantForm
                            {
                                Id = long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssfff")),
                                MerchantCode = merchantCode,
                                MerchantName = merchant?.BusinessName,
                                TemplateName = pdf_nuvei_type,
                                PDF_URL = file_url,
                                CreateBy = cMem.FullName,
                                CreateAt = DateTime.UtcNow,
                                UpdateBy = cMem.FullName,
                                UpdateAt = DateTime.UtcNow,
                                SendAt = DateTime.UtcNow,
                                SendByAgent = cMem.MemberNumber + "|" + cMem.FullName + "|" + cMem.PersonalEmail,
                                Status = "uploaded",
                                forNUVEI = true
                            };
                            db.O_MerchantForm.Add(new_mf);
                            db.SaveChanges();
                            merchant_form = new_mf;
                        }
                        else
                        {
                            mf.Status = "uploaded";
                            mf.Signed = false;
                            mf.PDF_URL = file_url;
                            if (System.IO.File.Exists(Server.MapPath("/upload/merchant/Merchant_" + merchantCode + "/" + pdf_nuvei_type + "_" + merchantCode + "_signed.pdf")))
                            {
                                if (System.IO.File.Exists(Server.MapPath("/upload/merchant/Merchant_" + merchantCode + "/" + pdf_nuvei_type + "_" + merchantCode + "_signed_old.pdf")))
                                {
                                    System.IO.File.Delete(Server.MapPath("/upload/merchant/Merchant_" + merchantCode + "/" + pdf_nuvei_type + "_" + merchantCode + "_signed_old.pdf"));
                                }

                                System.IO.File.Move(Server.MapPath("/upload/merchant/Merchant_" + merchantCode + "/" + pdf_nuvei_type + "_" + merchantCode + "_signed.pdf"), Server.MapPath("/upload/merchant/Merchant_" + merchantCode + "/" + pdf_nuvei_type + "_" + merchantCode + "_signed_old.pdf"));
                            }
                            db.Entry(mf).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            merchant_form = mf;
                        }
                        templateName = pdf_nuvei_type;
                    }
                    else
                    {
                        file_url = "/Page/MerchantFormHandle/Form?t=" + templateName + "&mc=" + merchantCode + "&oc=" + orderCode + (templateName == "Recurring Payment ACH" ? ("&p=" + period) : "");
                    }
                }
                return Json(new object[] { true, file_url, file_signed_url, merchant_form?.Id, templateName, merchantCode, orderCode, Key });
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, ex.Message, ex.InnerException?.Message });
            }
        }

        public string FillMerchantFormPdf(string template_name, string merchantcode, object data)
        {
            string pdfTemplate = Server.MapPath("/Upload/Merchant/Template/" + template_name + ".pdf");
            string filepath = "/Upload/Merchant/Merchant_" + merchantcode;

            return AppLB.CommonFunc.FillPdf(pdfTemplate, filepath, data, template_name + "_" + merchantcode);
            //string pdfTemplate = Server.MapPath("/Upload/Merchant/Template/" + template_name + ".pdf");
            //string filepath = "/Upload/Merchant/Merchant_" + merchantcode + "/" + template_name + "_" + merchantcode + ".pdf";
            //DirectoryInfo d = new DirectoryInfo(Server.MapPath("~/upload/merchant/Merchant_" + merchantcode));
            //if (!d.Exists)
            //{
            //    d.Create();
            //}
            //string newFile = Path.Combine(Server.MapPath(filepath));
            //PdfReader pdfReader = new PdfReader(pdfTemplate);
            //// PdfStamper pdfStamper = new PdfStamper(pdfReader, new FileStream(newFile, FileMode.Create));
            //using (PdfStamper pdfStamper = new PdfStamper(pdfReader, new FileStream(newFile, FileMode.Create)))
            //{
            //    AcroFields pdfFormFields = pdfStamper.AcroFields;
            //    var json = JsonConvert.SerializeObject(data);
            //    json = json.Replace("true", "\"Yes\"").Replace("false", "\"No\"");
            //    var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            //    foreach (var item in dictionary)
            //    {
            //        pdfFormFields.SetField(item.Key, item.Value);
            //    }
            //    pdfStamper.FormFlattening = true;

            //}
            //pdfReader.Close();
            //return filepath;
        }

        public JsonResult DownloadtoUpdate(string pdf_type, string merchantCode, bool newfile = false)
        {
            try
            {
                if (pdf_type != "Nuvei M2M Merchant Application" && pdf_type != "Nuvei 3 year Merchant Application")
                {
                    throw new Exception(pdf_type + " not found");
                }
                string fileName = "/upload/merchant/Merchant_" + merchantCode + "/" + pdf_type + "_" + merchantCode + ".pdf";
                string newfileName = "/upload/merchant/Merchant_" + merchantCode + "/" + pdf_type + "_" + merchantCode + "_new.pdf";
                DirectoryInfo d = new DirectoryInfo(Server.MapPath("~/upload/merchant/Merchant_" + merchantCode));
                if (!d.Exists) { d.Create(); }
                if (!(new FileInfo(Server.MapPath(fileName)).Exists) || newfile)
                {
                    if (newfile)
                    {
                        new FileInfo(Server.MapPath(newfileName)).Delete();
                        fileName = newfileName;
                    }
                    string pdfTemplate = Server.MapPath("/upload/merchant/template/" + pdf_type + ".pdf");
                    string newFile = Path.Combine(Server.MapPath(fileName));
                    C_Customer merchant = db.C_Customer.Where(c => c.CustomerCode == merchantCode).FirstOrDefault();

                    PdfReader pdfReader = new PdfReader(pdfTemplate);
                    using (PdfStamper pdfStamper = new PdfStamper(pdfReader, new FileStream(newFile, FileMode.Create)))
                    {
                        AcroFields pdfFormFields = pdfStamper.AcroFields;
                        //set form pdfFormFields
                        if (pdf_type == "Nuvei M2M Merchant Application")
                        {
                            pdfFormFields.SetField("legal_name", merchant.LegalName);
                            pdfFormFields.SetField("corporate_address_street", merchant.BusinessAddress);
                            pdfFormFields.SetField("corporate_city", merchant.BusinessCity);
                            pdfFormFields.SetField("corporate_state", merchant.BusinessState);
                            pdfFormFields.SetField("corporate_zip", merchant.BusinessZipCode);
                            pdfFormFields.SetField("corporate_telephone", merchant.BusinessPhone);

                            if (!string.IsNullOrEmpty(merchant.FederalTaxId))
                            {
                                for (int i = 0; i < merchant.FederalTaxId?.Length; i++)
                                {
                                    pdfFormFields.SetField("federal_tax_id_char:" + i, merchant.FederalTaxId[i].ToString());
                                }
                            }
                            pdfFormFields.SetField("dba_name", merchant.BusinessName);
                            pdfFormFields.SetField("location_address_street", merchant.BusinessAddress);
                            pdfFormFields.SetField("location_city", merchant.BusinessCity);
                            pdfFormFields.SetField("location_state", merchant.BusinessState);
                            pdfFormFields.SetField("location_zip", merchant.BusinessZipCode);
                            pdfFormFields.SetField("location_telephone", merchant.BusinessPhone);
                            pdfFormFields.SetField("owner_first_name_1", merchant.OwnerName.Split(' ').First());
                            pdfFormFields.SetField("owner_last_name_1", merchant.OwnerName.Split(' ').Last());
                            pdfFormFields.SetField("owner_email_1", merchant.Email);
                            pdfFormFields.SetField("owner_percent_ownership_1", merchant.Percentage.ToString());
                            pdfFormFields.SetField("owner_birthday_1", merchant.Birthday.HasValue ? merchant.Birthday.Value.ToString("mm/dd/yyyy") : "");
                            pdfFormFields.SetField("owner_residence_address_1_street", merchant.OwnerAddressStreet);
                            pdfFormFields.SetField("owner_city_1", merchant.City);
                            pdfFormFields.SetField("owner_zip_1", merchant.Zipcode);
                            pdfFormFields.SetField("owner_telephone_1", merchant.OwnerMobile);
                            if (!string.IsNullOrEmpty(merchant.SocialSecurity))
                            {
                                for (int i = 0; i < merchant.SocialSecurity.Length; i++)
                                {
                                    pdfFormFields.SetField("owner_social_security_1_char:" + i, merchant.SocialSecurity[i].ToString());
                                }
                            }
                        }
                        else if (pdf_type == "Nuvei 3 year Merchant Application")
                        {
                            pdfFormFields.SetField("legal_name", merchant.LegalName);
                            pdfFormFields.SetField("corporate_address_street", merchant.BusinessAddress);
                            pdfFormFields.SetField("corporate_city", merchant.BusinessCity);
                            pdfFormFields.SetField("corporate_state", merchant.BusinessState);
                            pdfFormFields.SetField("corporate_zip", merchant.BusinessZipCode);
                            pdfFormFields.SetField("corporate_telephone", merchant.BusinessPhone);

                            if (!string.IsNullOrEmpty(merchant.FederalTaxId))
                            {
                                for (int i = 0; i < merchant.FederalTaxId?.Length && i < 9; i++)
                                {
                                    pdfFormFields.SetField("federal_tax_id_char:" + i, merchant.FederalTaxId[i].ToString());
                                }
                            }
                            pdfFormFields.SetField("dba_name", merchant.BusinessName);
                            pdfFormFields.SetField("location_address_street", merchant.BusinessAddress);
                            pdfFormFields.SetField("location_city", merchant.BusinessCity);
                            pdfFormFields.SetField("location_state", merchant.BusinessState);
                            pdfFormFields.SetField("location_zip", merchant.BusinessZipCode);
                            pdfFormFields.SetField("location_telephone", merchant.BusinessPhone);
                            pdfFormFields.SetField("owner_first_name_1", merchant.OwnerName.Split(' ').First());
                            pdfFormFields.SetField("owner_last_name_1", merchant.OwnerName.Split(' ').Last());
                            pdfFormFields.SetField("owner_email_1", merchant.Email);
                            pdfFormFields.SetField("owner_percent_ownership_1", merchant.Percentage.ToString());
                            pdfFormFields.SetField("owner_birthday_1", merchant.Birthday.HasValue ? merchant.Birthday.Value.ToString("mm/dd/yyyy") : "");
                            pdfFormFields.SetField("owner_residence_address_1_street", merchant.OwnerAddressStreet);
                            pdfFormFields.SetField("owner_city_1", merchant.City);
                            pdfFormFields.SetField("owner_zip_1", merchant.Zipcode);
                            pdfFormFields.SetField("owner_telephone_1", merchant.OwnerMobile);
                            if (!string.IsNullOrEmpty(merchant.SocialSecurity))
                            {
                                for (int i = 0; i < merchant.SocialSecurity.Length; i++)
                                {
                                    pdfFormFields.SetField("owner_social_security_1_char:" + i, merchant.SocialSecurity[i].ToString());
                                }
                            }
                        }
                        pdfStamper.FormFlattening = false;
                    }
                    pdfReader.Close();
                }
                return Json(new object[] { true, fileName });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });
            }


        }
        public JsonResult SaveNuveiPDF(string merchantCode, string Type)
        {
            try
            {
                System.IO.File.Delete(Server.MapPath("/upload/merchant/Merchant_" + merchantCode + "/" + Type + "_" + merchantCode + ".pdf"));
                System.IO.File.Move(Server.MapPath("/upload/merchant/Merchant_" + merchantCode + "/view_" + Type + "_" + merchantCode + ".pdf"), Server.MapPath("/upload/merchant/Merchant_" + merchantCode + "/" + Type + "_" + merchantCode + ".pdf"));
                var mf = db.O_MerchantForm.Where(m => m.MerchantCode == merchantCode && m.TemplateName.Contains(Type)).FirstOrDefault();
                if (mf == null)
                {
                    var merchant = db.C_Customer.Where(c => c.CustomerCode == merchantCode).FirstOrDefault();
                    var merchant_form = new O_MerchantForm
                    {
                        Id = long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssfff")),
                        MerchantCode = merchantCode,
                        MerchantName = merchant?.BusinessName,
                        TemplateName = Type,
                        PDF_URL = "/upload/merchant/Merchant_" + merchantCode + "/" + Type + "_" + merchantCode + ".pdf",
                        CreateBy = cMem.FullName,
                        CreateAt = DateTime.UtcNow,
                        UpdateBy = cMem.FullName,
                        UpdateAt = DateTime.UtcNow,
                        SendAt = DateTime.UtcNow,
                        SendByAgent = cMem.MemberNumber + "|" + cMem.FullName + "|" + cMem.PersonalEmail,
                        Status = "uploaded",
                        forNUVEI = true
                    };
                    db.O_MerchantForm.Add(merchant_form);
                    db.SaveChanges();
                }
                return Json(new object[] { true, "Save completed!" });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });
            }



        }
        /// <summary>
        /// [Send to merchant when view] OR [Create & Send to merchant when create file]
        /// </summary>
        /// <param name="Key">["view": when view file] OR ["" or "null": when create file]</param>
        /// <param name="templateName"></param>
        /// <param name="merchantCode"></param>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public JsonResult SendPDFtoMerchant(string Key, string templateName, string merchantCode, string orderCode, string period)
        {
            orderCode = orderCode == "null" ? "" : orderCode;
            templateName = string.IsNullOrEmpty(orderCode) ? templateName : templateName?.Split('(')[0]?.Trim();
            string errMsg = string.Empty;
            string filePath = "";
            var result = "";

            try
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


                long? orderId = db.O_Orders.Where(o => o.OrdersCode == orderCode).FirstOrDefault()?.Id ?? (-1);
                var merchant = db.C_Customer.Where(c => c.CustomerCode == merchantCode).FirstOrDefault();

                var merchant_form = (from mf in db.O_MerchantForm.AsEnumerable()
                                     where mf.MerchantCode == merchantCode
                                     && mf.TemplateName.Contains(templateName)
                                     && (orderId == -1 || mf.OrderId == orderId)
                                     select mf).FirstOrDefault();


                #region Get filePath
                Form_CustomizeRequest customizeForm = new Form_CustomizeRequest();

                if (Key.Equals("view", StringComparison.OrdinalIgnoreCase) == true)
                {
                    //send
                    if (merchant_form?.Signed == true)
                    {
                        throw new Exception("-1");
                    }

                    if (System.IO.File.Exists(Server.MapPath(merchant_form?.PDF_URL)) == false)
                    {
                        throw new Exception("File url not found.");
                    }
                    filePath = merchant_form.PDF_URL;
                }
                else
                {
                    if (templateName.Equals("RequestForm"))
                    {
                        //create & send
                        customizeForm = FormCustomizeHandle.UpdateCustomizeForm(merchantCode, cMem.MemberNumber, out string err);
                        if (customizeForm == null)
                        {
                            throw new Exception(err);
                        }
                    }
                    else
                    {
                        result = UpdateMerchantInfo(templateName, merchantCode, orderId);
                        if (result != "")
                        {
                            throw new Exception(result);
                        }
                    }

                    result = ConvertHtmlToPdf(templateName, merchantCode, orderCode, period);
                    if (result.Contains("ok|") == false)
                    {
                        throw new Exception(result);
                    }


                    filePath = result.Replace("ok|", "");
                }
                #endregion


                #region Save to Database
                if (merchant_form != null)
                {
                    merchant_form.UpdateBy = cMem.FullName;
                    merchant_form.UpdateAt = DateTime.UtcNow;
                    merchant_form.SendAt = DateTime.UtcNow;
                    merchant_form.SendByAgent = cMem.MemberNumber + "|" + cMem.FullName + "|" + cMem.PersonalEmail;
                    db.Entry(merchant_form).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    if (templateName == "RequestForm")
                    {
                        templateName = customizeForm.Title + "-" + DateTime.UtcNow.ToString("MMMM dd,yyyy");
                    }

                    merchant_form = new O_MerchantForm
                    {
                        Id = long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssfff")),
                        MerchantCode = merchantCode,
                        MerchantName = merchant?.BusinessName,
                        TemplateName = templateName + (string.IsNullOrEmpty(orderCode) == true ? "" : " (Order #" + orderCode + ")"),
                        PDF_URL = filePath,
                        CreateBy = cMem.FullName,
                        CreateAt = DateTime.UtcNow,
                        UpdateBy = cMem.FullName,
                        UpdateAt = DateTime.UtcNow,
                        SendAt = DateTime.UtcNow,
                        SendByAgent = cMem.MemberNumber + "|" + cMem.FullName + "|" + cMem.PersonalEmail,
                        Status = "uploaded",
                        Send = true
                    };

                    if (orderId != -1)
                    {
                        merchant_form.OrderId = orderId;
                    }

                    db.O_MerchantForm.Add(merchant_form);
                }
                //db.SaveChanges();
                #endregion


                #region Send Document

                string signer_name = string.IsNullOrEmpty(merchant.OwnerName) == false ? merchant.OwnerName : merchant.ContactName;
                string signer_email = string.IsNullOrEmpty(merchant.Email) == false ? merchant.Email : merchant.BusinessEmail;
                if (string.IsNullOrWhiteSpace(signer_email))
                {

                    throw new Exception("Merchant email is invalid. Please update your merchant profile");
                }
                string envelopeId = string.Empty;
                if (templateName.Contains("Nuvei"))
                {
                    string pdfTemplate = Server.MapPath(filePath);
                    string newfile_path = filePath.Insert(filePath.LastIndexOf('.'), "_flat");
                    string newFile = Server.MapPath(newfile_path);
                    PdfReader pdfReader = new PdfReader(pdfTemplate);
                    using (PdfStamper pdfStamper = new PdfStamper(pdfReader, new FileStream(newFile, FileMode.Create)))
                    {
                        AcroFields pdfFormFields = pdfStamper.AcroFields;
                        pdfStamper.FormFlattening = true;
                    }
                    envelopeId = AppLB.DocuSign.DocuSignRestAPI.SendDocument(accessToken, newfile_path, signer_name, signer_email, out errMsg);
                    pdfReader.Close();
                }
                else
                {
                    envelopeId = AppLB.DocuSign.DocuSignRestAPI.SendDocument(accessToken, filePath, signer_name, signer_email, out errMsg);
                }
                if (string.IsNullOrEmpty(envelopeId))
                {
                    throw new Exception(errMsg);
                }
                #endregion

                //if send docusign ok.update merchant form.
                merchant_form.AgreementId = envelopeId;
                merchant_form.Status = "sent";
                merchant_form.Signed = false;

                if (customizeForm != null && string.IsNullOrEmpty(customizeForm.CustomerCode) == false)
                {
                    customizeForm.Status = "sent";
                    db.Entry(customizeForm).State = System.Data.Entity.EntityState.Modified;
                }
                db.SaveChanges();


                return Json(new object[] { true, "Send file success!" });
            }
            catch (Exception ex)
            {
                db.SaveChanges();
                return Json(new object[] { false, ex.Message });
            }
        }
        public JsonResult CreateNewPDF(string TemplateName, string MerchantCode)
        {

            try
            {
                long Id = 0;
                if (TemplateName.Contains("Priority Merchant Agreement"))
                {
                    var mer = db.C_Customer.Where(c => c.CustomerCode == MerchantCode).FirstOrDefault();
                    if (mer == null)
                    {
                        throw new AppHandleException("Merchant not found");
                    }
                    var data = new FormModel.Priority_Data
                    {
                        BusinessName = mer.BusinessName,
                        LocationAddress = CommonFunc.getStringValid(mer.SalonAddress1, mer.BusinessAddressStreet),
                        LCity = CommonFunc.getStringValid( mer.SalonCity, mer.BusinessCity),
                        LState = CommonFunc.getStringValid(mer.SalonState, mer.BusinessState),
                        LZip = CommonFunc.getStringValid(mer.SalonZipcode,mer.BusinessZipCode),
                        LPhone = CommonFunc.getStringValid(mer.SalonPhone,mer.BusinessPhone),
                        LegalName = mer.LegalName,
                        CorporateAddress = CommonFunc.getStringValid(mer.SalonAddress1, mer.BusinessAddressStreet),
                        CCity = CommonFunc.getStringValid(mer.SalonCity, mer.BusinessCity),
                        CState = CommonFunc.getStringValid(mer.SalonState, mer.BusinessState),
                        CZip = CommonFunc.getStringValid(mer.SalonZipcode, mer.BusinessZipCode),
                        ContactPhone = CommonFunc.getStringValid(mer.CellPhone, mer.SalonPhone, mer.BusinessPhone),
                        ContactName = mer.ContactName,
                        BusinessEmail = CommonFunc.getStringValid(mer.SalonEmail, mer.BusinessEmail),
                        OwnerName = mer.OwnerName,
                        OwnerAddress = mer.OwnerAddressStreet,
                        OCity = mer.City,
                        OState = mer.State,
                        OZip = mer.Zipcode,
                        OEmail = mer.Email,
                        SocialSecurity = mer.SocialSecurity,
                        DepositBankName = mer.DepositBankName,
                        BankRouting = mer.DepositRoutingNumber,
                        BankAccount = mer.DepositAccountNumber,
                        ACH_Individual = true,
                    };
                    string pdfTemplate;
                    string savepath;
                    string filepath;
                    if(TemplateName == "Priority Merchant Agreement_ IC+")
                    {
                        pdfTemplate = Server.MapPath("/Upload/Merchant/Template/Priority_Merchant_Agreement_IC.pdf");
                        savepath = "/Upload/Merchant/Merchant_" + MerchantCode;
                        filepath = CommonFunc.FillPdf(pdfTemplate, savepath, data, "Priority_Merchant_Agreement_IC_" + MerchantCode);
                    }
                    else if(TemplateName== "Priority Merchant Agreement_ Tiered")
                    {
                        pdfTemplate = Server.MapPath("/Upload/Merchant/Template/Priority_Merchant_Agreement_Tiered.pdf");
                        savepath = "/Upload/Merchant/Merchant_" + MerchantCode;
                        filepath = CommonFunc.FillPdf(pdfTemplate, savepath, data, "Priority_Merchant_Agreement_Tiered_" + MerchantCode);
                    }
                    else
                    {
                        pdfTemplate = Server.MapPath("/Upload/Merchant/Template/Priority_Merchant_Agreement_FlatRate.pdf");
                        savepath = "/Upload/Merchant/Merchant_" + MerchantCode;
                        filepath = CommonFunc.FillPdf(pdfTemplate, savepath, data, "Priority_Merchant_Agreement_FlatRate_" + MerchantCode);
                    }
                    Id = SaveMerchantForm(TemplateName, MerchantCode, filepath);

                }
                return Json(new object[] { true, Id });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message, JsonConvert.SerializeObject(e) });
            }


        }
        public JsonResult UploadUpdatedPdf(string TemplateName, string MerchantCode)
        {
            try
            {
                long Id = 0;
                string filename = MerchantCode + "_" + DateTime.UtcNow.ToString("yyMMdd");
                string savepath = "/upload/merchant/merchant_" + MerchantCode;
                if (TemplateName == "Priority")
                {
                    filename = "Priority_Merchant_" + filename;
                }
                else if (TemplateName == "BANK CHANGE FORM")
                {
                    filename = "ChangeBank_" + filename;
                }
                else if (TemplateName == "CHANGE REQUEST FORM")
                {
                    filename = "ChangeRequest_" + filename;
                }
                DirectoryInfo d = new DirectoryInfo(Server.MapPath(savepath));
                if (!d.Exists)
                {
                    d.Create();
                }
                else
                {
                    var n = d.GetFiles(filename + "*.pdf").Length + 1;
                    filename += "_" + n;
                }
                UploadAttachFile(savepath, "upload_file", "", filename + ".pdf", out string filepath);
                Id = SaveMerchantForm(TemplateName, MerchantCode, filepath);
                return Json(new object[] { true, "Upload Updated PDF completed!", Id });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message, JsonConvert.SerializeObject(e) });
            }
        }
        public long SaveMerchantForm(string TemplateName, string MerchantCode, string filepath, long? orderId = null)
        {
            var mer = db.C_Customer.Where(c => c.CustomerCode == MerchantCode).FirstOrDefault();
            var merchant_form = (from mf in db.O_MerchantForm.AsEnumerable()
                                 where mf.MerchantCode == MerchantCode
                                 && mf.TemplateName.Contains(TemplateName)
                                 select mf).FirstOrDefault();
            if (merchant_form != null)
            {
                merchant_form.UpdateBy = cMem.FullName;
                merchant_form.UpdateAt = DateTime.UtcNow;
                merchant_form.SendAt = DateTime.UtcNow;
                merchant_form.SendByAgent = cMem.MemberNumber + "|" + cMem.FullName + "|" + cMem.PersonalEmail;
                merchant_form.PDF_URL = filepath;
                db.Entry(merchant_form).State = System.Data.Entity.EntityState.Modified;
            }
            else
            {
                merchant_form = new O_MerchantForm
                {
                    Id = long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssfff")),
                    MerchantCode = MerchantCode,
                    MerchantName = mer?.BusinessName,
                    TemplateName = TemplateName,
                    PDF_URL = filepath,
                    CreateBy = cMem.FullName,
                    CreateAt = DateTime.UtcNow,
                    UpdateBy = cMem.FullName,
                    UpdateAt = DateTime.UtcNow,
                    SendAt = DateTime.UtcNow,
                    SendByAgent = cMem.MemberNumber + "|" + cMem.FullName + "|" + cMem.PersonalEmail,
                    Status = "uploaded",
                    //Send = true
                };
                db.O_MerchantForm.Add(merchant_form);
            }
            if (orderId > 0)
            {
                merchant_form.OrderId = orderId;
            }
            //db.SaveChanges();
            db.SaveChanges();
            return merchant_form.Id;
        }
        public JsonResult DownloadSendPDF(string Key, string templateName, string merchantCode, string orderCode, string period)
        {
            orderCode = orderCode == "null" ? "" : orderCode;
            templateName = string.IsNullOrEmpty(orderCode) ? templateName : templateName?.Split('(')[0]?.Trim();
            string errMsg = string.Empty;
            string filePath = "";
            var result = "";

            try
            {
                long? orderId = db.O_Orders.Where(o => o.OrdersCode == orderCode).FirstOrDefault()?.Id ?? (-1);
                var merchant = db.C_Customer.Where(c => c.CustomerCode == merchantCode).FirstOrDefault();

                var merchant_form = (from mf in db.O_MerchantForm.AsEnumerable()
                                     where mf.MerchantCode == merchantCode
                                     && mf.TemplateName.Contains(templateName)
                                     && (orderId == -1 || mf.OrderId == orderId)
                                     select mf).FirstOrDefault();


                #region Get filePath
                Form_CustomizeRequest customizeForm = new Form_CustomizeRequest();

                if (Key.Equals("view", StringComparison.OrdinalIgnoreCase) == true)
                {
                    //send
                    if (merchant_form?.Signed == true)
                    {
                        throw new Exception("-1");
                    }

                    if (System.IO.File.Exists(Server.MapPath(merchant_form?.PDF_URL)) == false)
                    {
                        throw new Exception("File url not found.");
                    }
                    filePath = merchant_form.PDF_URL;
                    return Json(new object[] { true, "", filePath });
                }
                else
                {
                    if (templateName.Equals("RequestForm"))
                    {
                        //create & send
                        customizeForm = FormCustomizeHandle.UpdateCustomizeForm(merchantCode, cMem.MemberNumber, out string err);
                        if (customizeForm == null)
                        {
                            throw new Exception(err);
                        }
                    }
                    else
                    {
                        result = UpdateMerchantInfo(templateName, merchantCode, orderId);
                        if (result != "")
                        {
                            throw new Exception(result);
                        }
                    }

                    if (templateName == "BANK CHANGE FORM")
                    {
                        result = FillMerchantFormPdf("ChangeBank", merchantCode, Session[BankChangeFormData]);
                    }
                    else
                    if (templateName == "CHANGE REQUEST FORM")
                    {
                        result = FillMerchantFormPdf("ChangeRequest", merchantCode, Session[NameChangeFormData]);
                    }
                    else
                    {
                        result = ConvertHtmlToPdf(templateName, merchantCode, orderCode, period, true);
                        if (result.Contains("ok|") == false)
                        {
                            throw new Exception(result);
                        }
                    }



                    filePath = result.Replace("ok|", "");
                }
                #endregion
                if (templateName == "RequestForm")
                {
                    templateName = customizeForm.Title + "-" + DateTime.UtcNow.ToString("MMMM dd,yyyy");
                }
                SaveMerchantForm(templateName, merchantCode, filePath, orderId);

                return Json(new object[] { true, "Create file pdf completed!", filePath });
            }
            catch (Exception ex)
            {
                db.SaveChanges();
                return Json(new object[] { false, ex.Message, JsonConvert.SerializeObject(ex) });
            }
        }

        /// <summary>
        /// Convert HTML to PDF, Save and return file path
        /// </summary>
        /// <param name="templateName"></param>
        /// <param name="merchantCode"></param>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public string ConvertHtmlToPdf(string templateName, string merchantCode, string orderCode, string period, bool download = false)
        {
            orderCode = orderCode == "null" ? "" : orderCode;
            try
            {
                // create a new pdf document converting an html string
                var url_page = Request.Url.Scheme + "://" + Request.Url.Authority + "/Page/MerchantFormHandle/Form?t=" + templateName + "&mc=" + merchantCode + "&oc=" + orderCode + "&u=true" + (templateName == "Recurring Payment ACH" ? ("&p=" + period) : "");
                if (templateName.Equals("RequestForm"))
                {
                    url_page = Request.Url.Scheme + "://" + Request.Url.Authority + "/Page/Form?mc=" + merchantCode + "&mn=" + cMem.MemberNumber + "&t=view";
                }

                // save pdf document
                #region check path
                System.IO.DirectoryInfo d = new System.IO.DirectoryInfo(Server.MapPath("~/upload/merchant/Merchant_" + merchantCode));
                if (!d.Exists)
                {
                    d.Create();
                }
                #endregion

                string strFileName = (download ? "download_" : "") + templateName.Replace(" ", "") + (string.IsNullOrEmpty(orderCode) == false ? "_" + orderCode : "") + ".pdf";
                if (templateName.Equals("RequestForm"))
                {
                    strFileName = templateName.Replace(" ", "") + "_" + merchantCode + "_" + cMem.MemberNumber + "_" + DateTime.UtcNow.ToString("yyMMddHHmmss") + ".pdf";
                }
                string strPath = "/upload/merchant/Merchant_" + merchantCode + "/" + strFileName;

                #region Check file Signed exist
                if (System.IO.File.Exists(Server.MapPath(strPath.Replace(".pdf", "_signed.pdf"))) == true)
                {
                    if (System.IO.File.Exists(Server.MapPath(strPath.Replace(".pdf", "_signed_old.pdf"))) == true)
                    {
                        System.IO.File.Delete(Server.MapPath(strPath.Replace(".pdf", "_signed_old.pdf")));
                    }

                    System.IO.File.Move(Server.MapPath(strPath.Replace(".pdf", "_signed.pdf")), Server.MapPath(strPath.Replace(".pdf", "_signed_old.pdf")));
                }
                #endregion


                HtmlToPdf converter = new HtmlToPdf();
                converter.Options.PdfPageSize = PdfPageSize.A4;
                converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;
                converter.Options.MarginLeft = 20;
                converter.Options.MarginRight = 20;
                converter.Options.MarginTop = 20;
                converter.Options.MarginBottom = 20;
                converter.Options.WebPageWidth = 1024;
                SelectPdf.PdfDocument doc = converter.ConvertUrl(url_page);//convert html to pdf
                string fullPath = System.IO.Path.Combine(Server.MapPath(strPath));
                doc.Save(fullPath);

                // close pdf document
                doc.Close();
                return "ok|" + strPath;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// Update merchant info before send
        /// </summary>
        /// <param name="templateName"></param>
        /// <param name="merchantCode"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public string UpdateMerchantInfo(string templateName, string merchantCode, long? orderId)
        {
            try
            {
                var mer = db.C_Customer.Where(c => c.CustomerCode == merchantCode).FirstOrDefault();

                if (templateName.Equals("BANK CHANGE FORM", StringComparison.OrdinalIgnoreCase))
                {
                    #region Bank Chang Form
                    if (Session[BankChangeFormData] == null)
                    {
                        throw new Exception("Session expired");
                    }

                    var data = Session[BankChangeFormData] as FormModel.BankChangeForm_Data;
                    //var address = data.Address?.Split(',');

                    mer.OwnerName = data.OwnerName;
                    mer.BusinessName = mer.CompanyName = data.BusinessName;
                    mer.DepositAccountNumber = data.DepositAccountNumber;
                    mer.DepositBankName = data.DepositBankName;
                    mer.DepositRoutingNumber = data.DepositRoutingNumber;
                    mer.WithdrawalAccountNumber = data.WithdrawalAccountNumber;
                    mer.WithdrawalBankName = data.WithdrawalBankName;
                    mer.WithdrawalRoutingNumber = data.WithdrawalRoutingNumber;
                    //mer. = data.MID;
                   // mer.DepositAccountNumber = data.DepositAccountNumber;


                    db.Entry(mer).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    #endregion
                }
                else if (templateName.Equals("CHANGE REQUEST FORM", StringComparison.OrdinalIgnoreCase))
                {
                    #region Name Chang Form
                    if (Session[NameChangeFormData] == null)
                    {
                        throw new Exception("Session expired");
                    }

                    var data = Session[NameChangeFormData] as FormModel.NameChangeForm_Data;
                    mer.OwnerName = data.OwnerName;
                    mer.LegalName = data.LegalName;
                    if (data.NewBusinessName_check) mer.BusinessName = data.NewBusinessName;
                    if (data.BusinessAddress_check)
                    {
                        mer.BusinessAddress = data.BusinessAddress;
                    }
                    if (data.EmailAddress_check)
                    {
                        mer.BusinessEmail = data.EmailAddress;
                    }
                    if (data.BusinessPhone_check)
                    {
                        mer.BusinessPhone = data.BusinessPhone;
                    }
                    if (data.Fax_check)
                    {
                        mer.Fax = data.Fax;
                    }
                    if (data.Website_check)
                    {
                        mer.Website = data.Website;
                    }
                    // var address = data.CompleteAddress?.Split(',');
                    //mer.OwnerName = data.OwnerName;
                    //mer.CellPhone = data.ContactPhone;
                    //mer.BusinessName = data.CurrentDBAname;
                    //mer.LegalName = data.CurrentLegalName;
                    //mer.FederalTaxId = data.FederalTaxID;
                    //mer.OwnerName = data.OwnerName;
                    //mer.BusinessAddressStreet = address[0]?.Trim() ?? "";
                    //mer.BusinessCity = address[1]?.Trim() ?? "";
                    //mer.BusinessState = address[2]?.Trim() ?? "";
                    //mer.BusinessZipCode = address[3]?.Trim() ?? "";
                    //mer.BusinessCountry = address[4]?.Trim() ?? "";
                    //Session["data_to_pdf"] = data;
                    //Session.Remove(NameChangeFormData);
                    db.Entry(mer).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    #endregion
                }
                else if (templateName.Equals("One-Time Payment ACH", StringComparison.OrdinalIgnoreCase) || templateName.Equals("Recurring Payment ACH", StringComparison.OrdinalIgnoreCase))
                {
                    if (Session[PaymentFormData] == null)
                    {
                        throw new Exception("Session expired");
                    }
                    var data = Session[PaymentFormData] as FormModel.PaymentForm_Data;
                    var order = db.O_Orders.Find(orderId);

                    #region Update Merchant
                    mer.OwnerName = data.OwnerName;
                    mer.LegalName = data.BusinessName;
                    mer.BusinessAddressStreet = data.BusinessStreetAddress;
                    mer.BusinessCity = data.BusinessCity?.Trim();
                    mer.BusinessState = data.BusinessState?.Trim();
                    mer.BusinessZipCode = data.BusinessZip?.Trim();
                    if (mer.CellPhone != null)
                    {
                        mer.CellPhone = data.Phone;
                    }
                    else
                    {
                        mer.BusinessPhone = data.Phone;
                    }

                    if (mer.Email != null)
                    {
                        mer.Email = data.Email;
                    }
                    else
                    {
                        mer.BusinessEmail = data.Email;
                    }
                    mer.DepositRoutingNumber = data.Routing;
                    mer.DepositAccountNumber = data.AccountNumber;
                    mer.DepositBankName = data.BankName;
                    db.Entry(mer).State = System.Data.Entity.EntityState.Modified;
                    #endregion

                    #region Update Order
                    if (templateName.Equals("One-Time Payment ACH", StringComparison.OrdinalIgnoreCase) == true
                        && DateTime.TryParse(data.Month + "/" + data.Day + "/" + data.Year, out DateTime invoiceDate) == true)
                    {
                        order.InvoiceDate = invoiceDate;
                        db.Entry(order).State = System.Data.Entity.EntityState.Modified;
                    }
                    #endregion

                    db.SaveChanges();
                }
                else if (templateName.Equals("Refund", StringComparison.OrdinalIgnoreCase))
                {
                    if (Session[RefundFormData] == null)
                    {
                        throw new Exception("Session expired");
                    }
                    var data = Session[RefundFormData] as FormModel.RefundForm_Data;

                    #region Update Merchant
                    mer.LegalName = data.BusinessName;
                    mer.OwnerName = data.OwnerName;
                    mer.BusinessAddressStreet = data.BusinessStreetAddress;
                    mer.BusinessCity = data.BusinessCity?.Trim();
                    mer.BusinessState = data.BusinessState?.Trim();
                    mer.BusinessZipCode = data.BusinessZip?.Trim();
                    db.Entry(mer).State = System.Data.Entity.EntityState.Modified;
                    #endregion

                    #region Create or Update O_Order_Refund
                    var refund = db.O_Order_Refund.Where(x => x.OrderCode == data.OrderCode).FirstOrDefault();
                    if (refund != null)//update
                    {
                        refund.RefundAmout = data.RefundAmount;
                        refund.Reason = data.Reason;
                        refund.Subject = data.Subject;
                        refund.Content = data.Content;
                        refund.UpdateAt = DateTime.UtcNow;
                        refund.UpdateBy = cMem.MemberNumber + "|" + cMem.FullName + "|" + cMem.PersonalEmail;
                        db.Entry(refund).State = System.Data.Entity.EntityState.Modified;
                    }
                    else//create
                    {
                        int countOfRefund = db.O_Order_Refund.Where(x => x.CreateAt.Value.Year == DateTime.Today.Year
                                           && x.CreateAt.Value.Month == DateTime.Today.Month).Count();

                        refund = new O_Order_Refund();
                        refund.Id = long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssfff"));
                        refund.Code = DateTime.UtcNow.ToString("yyMM") + (countOfRefund + 1).ToString().PadLeft(4, '0') + DateTime.UtcNow.ToString("ff");
                        refund.OrderCode = data.OrderCode;
                        refund.CustomerCode = data.MerchantCode;
                        refund.RefundAmout = data.RefundAmount ?? 0;
                        refund.Reason = data.Reason;
                        refund.Subject = data.Subject;
                        refund.Content = data.Content;
                        refund.CreateAt = DateTime.UtcNow;
                        refund.CreateBy = cMem.MemberNumber + "|" + cMem.FullName + "|" + cMem.PersonalEmail;
                        db.O_Order_Refund.Add(refund);
                    }
                    db.SaveChanges();
                    #endregion

                }

                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// Check the signed file or not. Return "signed" or "unsigned"
        /// </summary>
        /// <param name="templateName"></param>
        /// <param name="merchantCode"></param>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        public JsonResult CheckSigned(string templateName, string merchantCode, string orderCode)
        {
            try
            {
                if (templateName.Contains("Nuvei"))
                {
                    var sign = (from mf in db.O_MerchantForm.AsEnumerable()
                                where mf.TemplateName.Equals(templateName, StringComparison.OrdinalIgnoreCase)
                                && mf.MerchantCode == merchantCode && mf.forNUVEI == true
                                select mf).FirstOrDefault()?.Signed;
                    if (sign == true)
                    {
                        return Json("signed");
                    }
                }
                else
                {
                    orderCode = orderCode == "null" ? "" : orderCode;
                    long? orderId = db.O_Orders.Where(o => o.OrdersCode == orderCode).FirstOrDefault()?.Id ?? (-1);

                    var sign = (from mf in db.O_MerchantForm.AsEnumerable()
                                where mf.TemplateName.Equals(templateName + (string.IsNullOrEmpty(orderCode) == true ? "" : " (Order #" + orderCode + ")"), StringComparison.OrdinalIgnoreCase)
                                && mf.MerchantCode == merchantCode
                                && (orderId == -1 || mf.OrderId == orderId)
                                select mf).FirstOrDefault()?.Signed;
                    if (sign == true)
                    {
                        return Json("signed");
                    }
                }

                return Json("unsigned");
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }


        #region Change Data Form
        [HttpPost]
        public void ChangeData_BCF(FormModel.BankChangeForm_Data data)
        {
            Session[BankChangeFormData] = data;
        }
        [HttpPost]
        public void ChangeData_NCF(FormModel.NameChangeForm_Data data)
        {
            Session[NameChangeFormData] = data;
        }

        [HttpPost]
        public void ChangeData_OneTime(FormModel.PaymentForm_Data data)
        {
            var city_state_zip = Request["csz"]?.ToString();
            if (!string.IsNullOrEmpty(city_state_zip))
            {
                var content = city_state_zip.Split(',');

                for (int i = 0; i < content.Length; i++)
                {
                    if (i == 0) { data.BusinessCity = city_state_zip.Split(',')[i] ?? ""; }
                    if (i == 1) { data.BusinessState = city_state_zip.Split(',')[i] ?? ""; }
                    if (i == 2) { data.BusinessZip = city_state_zip.Split(',')[i] ?? ""; }
                }
            }
            Session[PaymentFormData] = data;
        }

        [HttpPost]
        public void ChangeData_Refund(FormModel.RefundForm_Data data)
        {
            data.Reason = Request["Reason"];
            data.Content = Request["Content"];
            var city_state_zip = Request["csz"]?.ToString();
            if (!string.IsNullOrEmpty(city_state_zip))
            {
                var content = city_state_zip.Split(',');

                for (int i = 0; i < content.Length; i++)
                {
                    if (i == 0) { data.BusinessCity = city_state_zip.Split(',')[i] ?? ""; }
                    if (i == 1) { data.BusinessState = city_state_zip.Split(',')[i] ?? ""; }
                    if (i == 2) { data.BusinessZip = city_state_zip.Split(',')[i] ?? ""; }
                }
            }
            Session[RefundFormData] = data;
        }
        #endregion
    }
}
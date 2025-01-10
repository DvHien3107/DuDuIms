using DataTables.AspNet.Core;
using Enrich.DataTransfer;
using Enrich.IServices;
using Enrich.IServices.Utils.Mailing;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Models.CustomizeModel;
using EnrichcousBackOffice.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace EnrichcousBackOffice.Controllers
{
    //public class MerchantFormController : Controller
    //{
    //    private Dictionary<string, bool> access = AppLB.Authority.GetAccessAuthority();
    //    private P_Member cMem = AppLB.Authority.GetCurrentMember();
    //    WebDataModel db = new WebDataModel();
    //    private readonly IMailingService _mailingService;
    //    private readonly IMerchantService _merchantService;

    //    public MerchantFormController(IMailingService mailingService, IMerchantService merchantService)
    //    {
    //        _mailingService = mailingService;
    //        _merchantService = merchantService;
    //    }

    //    // GET: MerchantForm
    //    public ActionResult Index()
    //    {
    //        if (access.Any(k => k.Key.Equals("form_manager")) == false || access["form_manager"] != true)
    //        {
    //            return Redirect("/home/forbidden");
    //        }
    //        return View();
    //    }
    //    public ActionResult ChangeTab(string TabName)
    //    {
    //        if (access.Any(k => k.Key.Equals("form_manager")) == false || access["form_manager"] != true)
    //        {
    //            return Redirect("/home/forbidden");
    //        }

    //        // add to view history top button
    //        UserContent.TabHistory = "Merchant form" + "|/merchantform";
    //        switch (TabName)
    //        {
    //            case "alreadysent":
    //                return PartialView("_Tab_AlreadySent");
    //            default:
    //                return PartialView("_Tab_FormLibrary");
    //        }
    //    }
    //    public ActionResult LoadListLibrary(IDataTablesRequest dataTablesRequest, string SearchText, int Status = -1)
    //    {
    //        var dataList = db.C_MerchantForm.Where(c => c.Status > -1).AsEnumerable();
    //        if (Status > -1)
    //        {
    //            dataList = dataList.Where(c => c.Status == Status);
    //        }
    //        if (!string.IsNullOrEmpty(SearchText))
    //        {
    //            dataList = dataList.Where(c => CommonFunc.SearchName(c.TemplateCode, SearchText) ||
    //                                                CommonFunc.SearchName(c.Description, SearchText) ||
    //                                                CommonFunc.SearchName(c.Name, SearchText));
    //        }

    //        var colSort = dataTablesRequest.Columns.Where(c => c.Sort != null).FirstOrDefault();
    //        switch (colSort.Name)
    //        {
    //            case "Code":
    //                if (colSort.Sort.Direction == SortDirection.Ascending)
    //                {
    //                    dataList = dataList.OrderBy(s => s.TemplateCode);
    //                }
    //                else
    //                {
    //                    dataList = dataList.OrderByDescending(s => s.TemplateCode);
    //                }
    //                break;
    //            case "Name":
    //                if (colSort.Sort.Direction == SortDirection.Ascending)
    //                {
    //                    dataList = dataList.OrderBy(s => s.Name);
    //                }
    //                else
    //                {
    //                    dataList = dataList.OrderByDescending(s => s.Name);
    //                }
    //                break;
    //            case "Description":
    //                if (colSort.Sort.Direction == SortDirection.Ascending)
    //                {
    //                    dataList = dataList.OrderBy(s => s.Description);
    //                }
    //                else
    //                {
    //                    dataList = dataList.OrderByDescending(s => s.Description);
    //                }
    //                break;
    //            case "Status":
    //                if (colSort.Sort.Direction == SortDirection.Ascending)
    //                {
    //                    dataList = dataList.OrderBy(s => s.Status);
    //                }
    //                else
    //                {
    //                    dataList = dataList.OrderByDescending(s => s.Status);
    //                }
    //                break;
    //            case "LastUpdated":
    //                if (colSort.Sort.Direction == SortDirection.Ascending)
    //                {
    //                    dataList = dataList.OrderBy(s => s.UpdatedAt);
    //                }
    //                else
    //                {
    //                    dataList = dataList.OrderByDescending(s => s.UpdatedAt);
    //                }
    //                break;
    //            default:
    //                dataList = dataList.OrderByDescending(s => s.TemplateCode);
    //                break;
    //        }

    //        var totalRecord = dataList.Count();
    //        dataList = dataList.Skip(dataTablesRequest.Start).Take(dataTablesRequest.Length);
    //        var dataView = dataList.ToList();
    //        return Json(new
    //        {
    //            recordsFiltered = totalRecord,
    //            recordsTotal = totalRecord,
    //            draw = dataTablesRequest.Draw,
    //            data = dataView
    //        });
    //    }
    //    public ActionResult Detail(string Id)
    //    {
    //        if (access.Any(k => k.Key.Equals("form_manager")) == false || access["form_manager"] != true)
    //        {
    //            TempData["e"] = "You don't have permission";
    //            return PartialView("_Partial_DetailAlreadySent", new C_MerchantForm { });
    //        }
    //        var _form = db.C_MerchantForm.Find(Id) ?? new C_MerchantForm() { };
    //        ViewBag.NextCode = MakeId();
    //        return PartialView("_Partial_DetailForm", _form);
    //    }
    //    public JsonResult Preview(string Id)
    //    {
    //        try
    //        {
    //            if (access.Any(k => k.Key.Equals("form_manager")) == false || access["form_manager"] != true)
    //            {
    //                throw new Exception("You don't have permission");
    //            }

    //            var _form = db.C_MerchantForm.Find(Id);
    //            if (_form == null) throw new Exception("Form library not found");
    //            return Json(new object[] { true, _form.ContentData }, JsonRequestBehavior.AllowGet);
    //        }
    //        catch (Exception e)
    //        {
    //            return Json(new object[] { false, "Error: " + e.Message }, JsonRequestBehavior.AllowGet);
    //        }
    //    }
    //    [HttpPost]
    //    [ValidateInput(false)]
    //    public JsonResult Save(C_MerchantForm _nForm)
    //    {
    //        try
    //        {
    //            if (access.Any(k => k.Key.Equals("form_manager")) == false || access["form_manager"] != true)
    //            {
    //                throw new Exception("You don't have permission");
    //            }

    //            if (string.IsNullOrEmpty(_nForm.Name)) throw new Exception("Please input Form name");
    //            if (db.C_MerchantForm.Where(c => c.Name == _nForm.Name && _nForm.Id != c.Id && c.Status > -1).Count() > 0) throw new Exception("Form name is available");
    //            var _form = db.C_MerchantForm.Find(_nForm.Id);
    //            if (_form == null) // create new
    //            {
    //                _nForm.Id = Guid.NewGuid().ToString();
    //                _nForm.TemplateCode = MakeId();
    //                _nForm.CreatedAt = _nForm.UpdatedAt = DateTime.UtcNow;
    //                _nForm.CreatedBy = _nForm.UpdatedBy = cMem.FullName;
    //                _nForm.Status = 0;
    //                db.C_MerchantForm.Add(_nForm);
    //                db.SaveChanges();
    //                return Json(new object[] { true, "Create Success", _nForm.Id });
    //            }
    //            else // edit
    //            {
    //                int _nStatus = Request["Status"] != null ? 1 : 0;
    //                _form.Name = _nForm.Name;
    //                _form.Description = _nForm.Description;
    //                _form.Status = _nStatus;
    //                _form.UpdatedBy = cMem.FullName;
    //                _form.UpdatedAt = DateTime.UtcNow;
    //                if (!string.IsNullOrEmpty(_nForm.ContentData))
    //                {
    //                    _form.ContentData = _nForm.ContentData;
    //                }
    //                db.Entry(_form).State = EntityState.Modified;
    //                db.SaveChanges();

    //                return Json(new object[] { true, "Update Success" });
    //            }
    //        }
    //        catch (Exception e)
    //        {
    //            return Json(new object[] { false, "Error: " + e.Message });
    //        }
    //    }
    //    public JsonResult Delete(string Id)
    //    {
    //        try
    //        {
    //            if (access.Any(k => k.Key.Equals("form_manager")) == false || access["form_manager"] != true)
    //            {
    //                throw new Exception("You don't have permission");
    //            }

    //            var form = db.C_MerchantForm.Find(Id);
    //            if (form == null) throw new Exception("Form library not found");
    //            form.Status = -1;
    //            form.UpdatedAt = DateTime.UtcNow;
    //            form.UpdatedBy = cMem.FullName;
    //            db.Entry(form).State = EntityState.Modified;
    //            db.SaveChanges();
    //            return Json(new object[] { true, "Delete Success" }, JsonRequestBehavior.AllowGet);
    //        }
    //        catch (Exception e)
    //        {
    //            return Json(new object[] { false, "Error: " + e.Message }, JsonRequestBehavior.AllowGet);
    //        }
    //    }
    //    public ActionResult Design(string key)
    //    {
    //        if (access.Any(k => k.Key.Equals("form_manager")) == false || access["form_manager"] != true)
    //        {
    //            throw new Exception("You don't have permission");
    //        }

    //        var template = db.C_MerchantForm.Find(key) ?? new C_MerchantForm { };
    //        if (template.Id == null)
    //        {
    //            TempData["e"] = "Form library not found";
    //        }
    //        return View(template);
    //    }
    //    public JsonResult GetFormData (string key)
    //    {
    //        try
    //        {
    //            if (access.Any(k => k.Key.Equals("form_manager")) == false || access["form_manager"] != true)
    //            {
    //                throw new Exception("You don't have permission");
    //            }
    //            var form = db.C_MerchantForm.Find(key);
    //            if (form == null) throw new Exception("Form library not found");
    //            return Json(new object[] { true, form.ContentData }, JsonRequestBehavior.AllowGet);
    //        }
    //        catch (Exception e)
    //        {
    //            return Json(new object[] { false, "Error: " + e.Message }, JsonRequestBehavior.AllowGet);
    //        }
    //    }
    //    public ActionResult Render(string key, string code, bool v = true)
    //    {
    //        try
    //        {
    //            ViewBag.viewAction = v;
    //            var _formData = db.C_MerchantFormData.Find(key);
    //            if (_formData != null && _formData?.CustomerCode == code)
    //            {
    //                return View(_formData);
    //            }
    //            else
    //            {
    //                var _form = db.C_MerchantForm.Find(key);
    //                if (_form == null) throw new Exception("Sorry! Page Not Found");
    //                return View(new C_MerchantFormData { ContentData = _form.ContentData });
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            TempData["e"] = ex.Message;
    //            return View();
    //        }
    //    }
    //    public ActionResult LoadListFormAlready(IDataTablesRequest dataTablesRequest, string SearchText, int Status = -1)
    //    {
    //        var dataList = db.C_MerchantFormData.Where(c => c.Status > -1)
    //            .Join(db.C_Customer.Where(c => !string.IsNullOrEmpty(c.CustomerCode)), fd => fd.CustomerCode, c => c.CustomerCode, (fd, c) => new { fd, c })
    //            .Join(db.C_MerchantForm.Where(c => c.Status > -1), fc => fc.fd.TemplateId, f => f.Id, (fc, f) => new { fc.c, fc.fd, f })
    //            .AsEnumerable();
    //        if (Status > -1)
    //        {
    //            dataList = dataList.Where(c => c.fd.Status == Status);
    //        }
    //        if (!string.IsNullOrEmpty(SearchText))
    //        {
    //            dataList = dataList.Where(c => CommonFunc.SearchName(c.fd.ContentAddon, SearchText) ||
    //                                                CommonFunc.SearchName(c.fd.CustomerCode, SearchText) ||
    //                                                CommonFunc.SearchName(c.fd.Subject, SearchText) ||
    //                                                CommonFunc.SearchName(c.fd.CreatedBy, SearchText) ||
    //                                                CommonFunc.SearchName(c.fd.SubmitedBy, SearchText) ||
    //                                                CommonFunc.SearchName(c.fd.SubmitedEmail, SearchText) ||
    //                                                CommonFunc.SearchName(c.fd.SentBy, SearchText) ||
    //                                                CommonFunc.SearchName(c.fd.EmailCC, SearchText) ||
    //                                                CommonFunc.SearchName(c.c.BusinessName, SearchText) ||
    //                                                CommonFunc.SearchName(c.f.Name, SearchText) ||
    //                                                CommonFunc.SearchName(c.f.TemplateCode, SearchText) ||
    //                                                CommonFunc.SearchName(c.f.Description, SearchText));
    //        }

    //        var colSort = dataTablesRequest.Columns.Where(c => c.Sort != null).FirstOrDefault();
    //        switch (colSort.Name)
    //        {
    //            case "TemplateCode":
    //                if (colSort.Sort.Direction == SortDirection.Ascending)
    //                {
    //                    dataList = dataList.OrderBy(s => s.f.TemplateCode);
    //                }
    //                else
    //                {
    //                    dataList = dataList.OrderByDescending(s => s.f.TemplateCode);
    //                }
    //                break;
    //            case "CustomerCode":
    //                if (colSort.Sort.Direction == SortDirection.Ascending)
    //                {
    //                    dataList = dataList.OrderBy(s => s.fd.CustomerCode);
    //                }
    //                else
    //                {
    //                    dataList = dataList.OrderByDescending(s => s.fd.CustomerCode);
    //                }
    //                break;
    //            case "Subject":
    //                if (colSort.Sort.Direction == SortDirection.Ascending)
    //                {
    //                    dataList = dataList.OrderBy(s => s.fd.Subject);
    //                }
    //                else
    //                {
    //                    dataList = dataList.OrderByDescending(s => s.fd.Subject);
    //                }
    //                break;
    //            case "ContentAddon":
    //                if (colSort.Sort.Direction == SortDirection.Ascending)
    //                {
    //                    dataList = dataList.OrderBy(s => s.fd.ContentAddon);
    //                }
    //                else
    //                {
    //                    dataList = dataList.OrderByDescending(s => s.fd.ContentAddon);
    //                }
    //                break;
    //            case "Status":
    //                if (colSort.Sort.Direction == SortDirection.Ascending)
    //                {
    //                    dataList = dataList.OrderBy(s => s.fd.Status);
    //                }
    //                else
    //                {
    //                    dataList = dataList.OrderByDescending(s => s.fd.Status);
    //                }
    //                break;
    //            case "History":
    //                if (colSort.Sort.Direction == SortDirection.Ascending)
    //                {
    //                    dataList = dataList.OrderBy(s => s.fd.SubmitedAt ?? s.fd.SentAt ?? s.fd.CreatedAt);
    //                }
    //                else
    //                {
    //                    dataList = dataList.OrderByDescending(s => s.fd.SubmitedAt ?? s.fd.SentAt ?? s.fd.CreatedAt);
    //                }
    //                break;
    //            default:
    //                dataList = dataList.OrderByDescending(s => s.fd.SubmitedAt ?? s.fd.SentAt ?? s.fd.CreatedAt);
    //                break;
    //        }
    //        var totalRecord = dataList.Count();
    //        dataList = dataList.Skip(dataTablesRequest.Start).Take(dataTablesRequest.Length);
    //        var dataView = dataList.ToList();
    //        return Json(new
    //        {
    //            recordsFiltered = totalRecord,
    //            recordsTotal = totalRecord,
    //            draw = dataTablesRequest.Draw,
    //            data = dataView
    //        });
    //    }
    //    public ActionResult DetailFormAlready(string Id)
    //    {
    //        if (access.Any(k => k.Key.Equals("form_for_customer")) == false || access["form_for_customer"] != true)
    //        {
    //            //return Redirect("/home/forbidden");
    //            TempData["e"] = "You don't have permission";
    //            return PartialView("_Partial_DetailAlreadySent", new C_MerchantFormData { });
    //        }

    //        var _form = db.C_MerchantFormData.Find(Id) ?? new C_MerchantFormData() { };
    //        ViewBag.Customers = db.C_Customer.Where(c => !string.IsNullOrEmpty(c.CustomerCode)).OrderByDescending(c => c.CustomerCode).ToList();
    //        ViewBag.Templates = db.C_MerchantForm.Where(c => c.Status == 1).OrderByDescending(c => c.TemplateCode).ToList();
    //        return PartialView("_Partial_DetailAlreadySent", _form);
    //    }
    //    [HttpPost]
    //    public JsonResult SaveFormAlready(C_MerchantFormData _nFormData)
    //    {
    //        try
    //        {
    //            if (access.Any(k => k.Key.Equals("form_for_customer")) == false || access["form_for_customer"] != true)
    //            {
    //                throw new Exception("You don't have permission");
    //            }

    //            if (db.C_MerchantFormData.Where(c => c.CustomerCode == _nFormData.CustomerCode && c.TemplateId == _nFormData.TemplateId && c.Id != _nFormData.Id && c.Status > -1).Count() > 0) throw new Exception("Form already for customer is available");
    //            var _formdata = db.C_MerchantFormData.Find(_nFormData.Id);
    //            var _form = db.C_MerchantForm.Find(_nFormData.TemplateId);
    //            if (_form == null) throw new Exception("Form library not found");
    //            if (_formdata == null) // create new
    //            {
    //                _nFormData.Id = Guid.NewGuid().ToString();
    //                _nFormData.CreatedAt = DateTime.UtcNow;
    //                _nFormData.CreatedBy = cMem.FullName;
    //                _nFormData.CreatedMember = cMem.MemberNumber;
    //                _nFormData.Status = 0;
    //                _nFormData.ContentData = _form.ContentData;
    //                db.C_MerchantFormData.Add(_nFormData);
    //                db.SaveChanges();
    //                return Json(new object[] { true, "Create Success" });
    //            }
    //            else // edit
    //            {
    //                //if (_formdata.Status > 0) throw new Exception("Cannot update because form already for customer has been sent");
    //                //int _nStatus = Request["Status"] != null ? int.Parse(Request["Status"]) : 0;
    //                _formdata.ContentAddon = _nFormData.ContentAddon;
    //                _formdata.ContentData = _form.ContentData;
    //                _formdata.CustomerCode = _nFormData.CustomerCode;
    //                _formdata.TemplateId = _nFormData.TemplateId;
    //                _formdata.EmailCC = _nFormData.EmailCC;
    //                _formdata.Subject = _nFormData.Subject;
    //                db.Entry(_form).State = EntityState.Modified;
    //                db.SaveChanges();
    //                return Json(new object[] { true, "Update Success" });
    //            }
    //        }
    //        catch (Exception e)
    //        {
    //            return Json(new object[] { false, "Error: " + e.Message });
    //        }
    //    }
    //    public JsonResult DeleteFormAlready(string Id)
    //    {
    //        try
    //        {
    //            if (access.Any(k => k.Key.Equals("form_for_customer")) == false || access["form_for_customer"] != true)
    //            {
    //                throw new Exception("You don't have permission");
    //            }

    //            var form = db.C_MerchantFormData.Find(Id);
    //            if (form == null) throw new Exception("Form already not found");
    //            form.Status = -1;
    //            db.Entry(form).State = EntityState.Modified;
    //            db.SaveChanges();
    //            return Json(new object[] { true, "Delete Success" }, JsonRequestBehavior.AllowGet);
    //        }
    //        catch (Exception e)
    //        {
    //            return Json(new object[] { false, "Error: " + e.Message }, JsonRequestBehavior.AllowGet);
    //        }
    //    }
    //    [HttpPost]
    //    public JsonResult SubmissionFormData(C_MerchantFormData _nFormData)
    //    {
    //        try
    //        {
    //            if (string.IsNullOrEmpty(_nFormData.Id)) return Json(new object[] { true });

    //            var _formdata = db.C_MerchantFormData.Find(_nFormData.Id);
    //            if (_formdata == null) throw new Exception();
    //            _formdata.Status = 2;
    //            _formdata.ContentData = _nFormData.ContentData;
    //            _formdata.SubmitedAt = DateTime.UtcNow;
    //            _formdata.SubmitedBy = Request["SubmitMember"];
    //            _formdata.SubmitedEmail = Request["SubmitEmail"];
    //            db.Entry(_formdata).State = EntityState.Modified;
    //            db.SaveChanges();
    //            var cus = db.C_Customer.FirstOrDefault(c => c.CustomerCode == _formdata.CustomerCode) ?? new C_Customer { };
    //             _merchantService.SaveHistoryUpdate(cus.CustomerCode, $"Merchant form has been submitted");
    //            new MerchantService().WriteLogMerchant(cus, "Merchant form has been submitted", $"Merchant form has been submitted. View data in here: <a target='_blank' href='/merchantform/render?key={_formdata.Id}&code={_formdata.CustomerCode}'>Form link</a>");
    //            SendMerchantFormToStaff(_formdata);
    //            return Json(new object[] { true });
    //        }
    //        catch (Exception e)
    //        {
    //            return Json(new object[] { false, "Error: " + e.Message });
    //        }
    //    }
    //    public async Task<JsonResult> SendMerchantFormToCustomer(C_MerchantFormData _nFormData)
    //    {
    //        try
    //        {
    //            if (access.Any(k => k.Key.Equals("form_for_customer")) == false || access["form_for_customer"] != true)
    //            {
    //                throw new Exception("You don't have permission");
    //            }

    //            if (db.C_MerchantFormData.Where(c => c.CustomerCode == _nFormData.CustomerCode && c.TemplateId == _nFormData.TemplateId && c.Id != _nFormData.Id && c.Status > -1).Count() > 0) throw new Exception("Form already for customer is available");
    //            var _formdata = db.C_MerchantFormData.Find(_nFormData.Id);
    //            var _form = db.C_MerchantForm.Find(_nFormData.TemplateId);
    //            if (_form == null) throw new Exception("Form library not found");
    //            var cus = db.C_Customer.Where(c => c.CustomerCode == _nFormData.CustomerCode).FirstOrDefault();
    //            if (cus == null) throw new Exception("Merchant not found");

    //            // save data form
    //            _formdata.ContentAddon = _nFormData.ContentAddon;
    //            _formdata.ContentData = _form.ContentData;
    //            _formdata.CustomerCode = _nFormData.CustomerCode;
    //            _formdata.TemplateId = _nFormData.TemplateId;
    //            _formdata.EmailCC = _nFormData.EmailCC;
    //            _formdata.Subject = _nFormData.Subject;
    //            _formdata.Status = 1;
    //            db.Entry(_form).State = EntityState.Modified;
    //            db.SaveChanges();

    //            //send email
    //            string emailResult = "n/a";
    //            string mfLink = string.Format("{0}://{1}/{2}", Request.Url.Scheme, Request.Url.Authority, $"merchantform/render?key={_formdata.Id}&code={_formdata.CustomerCode}&v=false");
    //            if (!string.IsNullOrWhiteSpace(cus.Email) || !string.IsNullOrWhiteSpace(cus.BusinessEmail))
    //            {
    //                string email = string.IsNullOrEmpty(cus.SalonEmail?.Trim() ?? "") ? (string.IsNullOrEmpty(cus.BusinessEmail?.Trim() ?? "") ? cus.Email : cus.BusinessEmail) : cus.SalonEmail;
    //                var email_data = new SendGridEmailTemplateData.EmailMerchantForm
    //                {
    //                    subject = _formdata.Subject,
    //                    contentaddon = _formdata.ContentAddon,
    //                    link = mfLink,
    //                    salon_name = cus.OwnerName
    //                };
    //                ReadXML xml = new ReadXML(System.Web.Hosting.HostingEnvironment.MapPath("/App_Data/Config.xml"));
    //                XmlNode node = xml.GetNode("/root/sendgrid_template/merchant_form");

    //                string to = cus.SalonEmail;
    //                string firstname = cus.BusinessName;
    //                string cc = string.Join(";", _formdata.EmailCC, cus.MangoEmail, cus.Email);
    //                emailResult = await _mailingService.SendBySendGridWithTemplate(to, firstname, node["template_id"].InnerText, cc, email_data);
    //            }

    //            _merchantService.SaveHistoryUpdate(cus.CustomerCode, "Send merchant form completed");
    //            new MerchantService().WriteLogMerchant(cus, "Send merchant form", "Send merchant form to customer has been completed");

    //            return Json(new object[] { true, "Email has ben sent" });
    //        }
    //        catch (Exception e)
    //        {
    //            return Json(new object[] { false, e.Message });
    //        }
    //    }
    //    public async Task<JsonResult> SendMerchantFormToStaff(C_MerchantFormData _formdata)
    //    {
    //        try
    //        {
    //            var member = db.P_Member.FirstOrDefault(c => c.MemberNumber == _formdata.CreatedMember);
    //            if (member == null) throw new Exception("Member not found");
    //            var customer = db.C_Customer.FirstOrDefault(c => c.CustomerCode == _formdata.CustomerCode);
    //            if (customer == null) throw new Exception("Customer not found");

    //            //send email
    //            string emailResult = "n/a";
    //            string mfLink = string.Format("{0}://{1}/{2}", Request.Url.Scheme, Request.Url.Authority, $"merchantform/render?key={_formdata.Id}&code={_formdata.CustomerCode}");

    //            string email = member.PersonalEmail ?? member.Email1 ?? member.Email2;
    //            var email_data = new SendGridEmailTemplateData.EmailMerchantFormToStaff
    //            {
    //                staff_name = member.FullName,
    //                salon_code = customer.CustomerCode,
    //                salon_name = customer.BusinessName,
    //                link = mfLink
    //            };
    //            ReadXML xml = new ReadXML(System.Web.Hosting.HostingEnvironment.MapPath("/App_Data/Config.xml"));
    //            XmlNode node = xml.GetNode("/root/sendgrid_template/merchant_form_staff");

    //            string to = email;
    //            string firstname = member.FullName;
    //            string cc = string.Empty;
    //            //string cc = string.Join(";", _formdata.EmailCC, cus.MangoEmail, cus.Email);
    //            emailResult = await _mailingService.SendBySendGridWithTemplate(to, firstname, node["template_id"].InnerText, cc, email_data);
    //            return Json(new object[] { true, "Email has ben sent" });
    //        }
    //        catch (Exception e)
    //        {
    //            return Json(new object[] { false, e.Message });
    //        }
    //    }
    //    private string MakeId()
    //    {
    //        return $"F{(int.Parse((db.C_MerchantForm.Where(c => c.TemplateCode.Substring(0, 1) == "F").Max(c => c.TemplateCode) ?? "F0").Substring(1)) + 1).ToString().PadLeft(5, '0')}";
    //    }
    //}
}
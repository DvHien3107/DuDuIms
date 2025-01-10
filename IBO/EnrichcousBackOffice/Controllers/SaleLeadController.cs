using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.ViewModel;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Utils;
using EnrichcousBackOffice.Utils.IEnums;
using EnrichcousBackOffice.ViewControler;
using System.Threading.Tasks;
using System.IO;
using EnrichcousBackOffice.Services;
using Inner.Libs.Helpful;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.Util;
using System.Web.Configuration;
using Newtonsoft.Json;
using EnrichcousBackOffice.Models.CustomizeModel;
using DataTables.AspNet.Core;
using System.Text.RegularExpressions;
using Org.BouncyCastle.Bcpg.OpenPgp;
using System.Web;
using System.Configuration;
using System.Net;
using RestSharp.Extensions;
using System.Xml;
using EnrichcousBackOffice.Services.Notifications;
using Enrich.Core.Infrastructure;
using Enrich.IServices.Utils.SMS;
using Enrich.IServices.Utils.Universal;
using NPOI.SS.Formula.Eval;
using Enrich.IServices.Utils.GoHighLevelConnector;
using Enrich.Core.Ultils;

namespace EnrichcousBackOffice.Controllers
{
    [MyAuthorize]
    public class SaleLeadController : Controller
    {
        private WebDataModel db = new WebDataModel();
        private P_Member cMem = Authority.GetCurrentMember();
        private Dictionary<string, bool> access = Authority.GetAccessAuthority();
        // GET: SaleLead
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// FindLead by customerCode
        /// </summary>
        /// <param name="member_number"></param>
        /// <param name="customerCode"></param>
        /// <returns></returns>
        private IEnumerable<SaleLeadInfo> FindLead(string member_number)
        {
            IEnumerable<SaleLeadInfo> leads = (
                from sl in db.C_SalesLead.AsEnumerable()
                where sl.SL_Status != null
                where (string.IsNullOrEmpty(member_number) && sl.MemberNumber == cMem.MemberNumber ||
                      !string.IsNullOrEmpty(member_number) && member_number == sl.MemberNumber || member_number == "all")
                join c in db.C_Customer on sl.CustomerCode equals c.CustomerCode
                join sl_status in db.C_SalesLead_Status on sl.SL_Status equals sl_status.Id
                select SaleLeadInfo.MakeSelect(c, sl, sl_status, true)
            ).OrderByDescending(c => c.UpdateAt).ToList();
            return leads;
        }
        private SaleLeadInfo getLead(string SalesLeadId)
        {
            SaleLeadInfo lead = (
                from sl in db.C_SalesLead.AsEnumerable()
                where sl.SL_Status != null
                where (!string.IsNullOrEmpty(SalesLeadId) && sl.Id == SalesLeadId)
                join c in db.C_Customer on sl.CustomerCode equals c.CustomerCode
                join sl_status in db.C_SalesLead_Status on sl.SL_Status equals sl_status.Order
                select SaleLeadInfo.MakeSelect(c, sl, sl_status, true)
            ).FirstOrDefault();
            return lead;
        }
        public ActionResult Search(string search_condition, string member, int? status, DateTime? fdate, DateTime? tdate)
        {
            if (member != cMem.MemberNumber && !string.IsNullOrEmpty(member) && (access.Any(k => k.Key.Equals("sale_lead_view_all")) != true || access["sale_lead_view_all"] != true))
            {
                ViewData["e"] = "You don't have permission to view All member!";
                return PartialView("_Partial_List_SaleLead");
            }
            var leads = (
                from l in FindLead(member)
                where CommonFunc.SearchName(
                  String.Join("", new[] {
                      l.ContactName,l.ContactTitle,l.Email,l.CellPhone,l.CompanyName,l.State,l.City,l.Country,l.CompanyEmail,l.Comment,l.CreateBy,l.UpdateBy,
                      l.BusinessName,l.BusinessPhone,l.BusinessEmail,l.BusinessState,l.BusinessCity,l.BusinessCountry,l.Website,l.PreferredLanguage,l.LegalName,
                      l.OwnerMobile,l.OwnerSocial,l.ContactAddress,l.BusinessAddressCivicNumber,l.BusinessAddressStreet,l.OwnerName,l.BusinessAddress,
                      l.OwnerAddressCivicNumber,l.OwnerAddressStreet,l.BusinessDescription,l.CurrentProcessorName,l.SalonAddress1,l.SalonAddress2,
                      l.SalonCity,l.SalonState,l.SalonPhone,l.SalonTimeZone,l.SalonEmail
                  }.Where(entry => (entry?.Trim() ?? "") != ""))
                  , search_condition, true
                )
                && (status == null || l.SL_Status == status)
                && (fdate == null || (l.UpdateAt ?? l.CreateAt).Value.Date >= fdate)
                && (tdate == null || (l.UpdateAt ?? l.CreateAt).Value.Date <= tdate)
                select l);
            return PartialView("_Partial_List_SaleLead", leads);
        }
        /// <summary>
        /// get list appoiment partial of lead customer by customer code
        /// </summary>
        /// <param name="code">customer code</param>
        /// <returns></returns>
        public ActionResult getAppoimentsPartial(string code, bool pr_access = true)
        {
            try
            {
                var currentDeparmentsUser = db.P_Department.Where(d => d.Type == "SALES" && d.GroupMemberNumber.Contains(cMem.MemberNumber));
                var ShowTeam = false;
                if ((access.Any(k => k.Key.Equals("sales_lead_assigned")) == true && access["sales_lead_assigned"] == true))
                {
                    var sale_dep = db.P_Department.Where(d => d.Type == "SALES").Select(d => d.Id).ToList().Select(d => d.ToString());
                    if (access.Any(k => k.Key.Equals("sales_lead_view_all")) == true && access["sales_lead_view_all"] == true)
                    {
                        ViewBag.ListMemberSales = db.P_Member.Where(m => m.Active == true && sale_dep.Any(n => m.DepartmentId.Contains(n))).ToList();
                        ShowTeam = true;
                    }
                    else if (access.Any(k => k.Key.Equals("sales_lead_view_team")) == true && access["sales_lead_view_team"] == true)
                    {
                        List<string> listMemberNumber = new List<string>();
                        var ManagerDep = db.P_Department.Where(x => x.SupervisorNumber == cMem.MemberNumber && x.ParentDepartmentId != null && x.Type == "SALES").ToList();
                        if (ManagerDep.Count() > 0)
                        {
                            foreach (var dep in ManagerDep)
                            {
                                listMemberNumber.AddRange(dep.GroupMemberNumber.Split(','));
                            }
                            ShowTeam = true;
                        }
                        if (currentDeparmentsUser.Count() > 0)
                        {
                            foreach (var deparment in currentDeparmentsUser)
                            {
                                if (deparment.LeaderNumber == cMem.MemberNumber)
                                {
                                    listMemberNumber.AddRange(deparment.GroupMemberNumber.Split(','));
                                }
                            }
                        }
                        listMemberNumber.Add(cMem.MemberNumber);
                        listMemberNumber = listMemberNumber.GroupBy(x => x).Select(x => x.Key).ToList();
                        ViewBag.ListMemberSales = (from m in db.P_Member join l in listMemberNumber on m.MemberNumber equals l select m).ToList();
                    }
                    ViewBag.ShowTeam = ShowTeam;
                }
                else
                {
                    var sale_dep = db.P_Department.Where(d => d.Type == "SALES").Select(d => d.Id).ToList().Select(d => d.ToString());
                    ViewBag.ListMemberSales = db.P_Member.Where(m => m.Active == true && sale_dep.Any(n => m.DepartmentId.Contains(n)) && m.MemberNumber.Contains(cMem.MemberNumber)).ToList();
                }

                IEnumerable<Calendar_Event> appointments = db.Calendar_Event.Where(a => a.SalesLeadId == code).OrderByDescending(a => a.StartEvent).ToList();

                ViewBag.pr_access = pr_access;
                var model = new DetailSalesLeadCustomizeModel
                {
                    even = appointments,
                    lead = getLead(code)
                };
                return PartialView("_Partial_List_Appoiment", model);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Failure! " + ex.Message;
                var model = new DetailSalesLeadCustomizeModel
                {
                    even = Enumerable.Empty<Calendar_Event>(),
                    lead = new SaleLeadInfo()
                };
                return PartialView("_Partial_List_Appoiment", model);
            }
        }

        [ValidateInput(false)]
        public JsonResult AppoimentSave(string lead_id, string CallOfNumber, int? StatusInteraction, string appointment_id, string title, string schedule_timezone, string date, string hours, string minute, string am_pm, string description, int? event_done, string Type)
        {
            try
            {
                string gmt = "";
                string start = "";
                if (Type == "Event")
                {
                    gmt = schedule_timezone.Substring(4, 6);
                    int hour = int.Parse(hours.PadLeft(2, '0'));
                    if (am_pm == "PM")
                    {
                        hour += 12;
                    }
                    start = DateTime.Parse(date).ToString("yyyy-MM-ddT") + hour.ToString().PadLeft(2, '0') + ":" + minute.PadLeft(2, '0') + ":00" + gmt;
                }
                else
                {
                    gmt = "+00:00";
                    start = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss") + gmt;
                }
                string message = string.Empty;
                var ap = new Calendar_Event();
                if (string.IsNullOrEmpty(appointment_id))
                {
                    var sl = db.C_SalesLead.Where(c => c.Id == lead_id).FirstOrDefault();
                    ap = new Calendar_Event
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = title,
                        Description = description,
                        StartEvent = start,
                        CreateAt = DateTime.UtcNow,
                        CreateBy = cMem.FullName,
                        Type = Type,
                        GMT = gmt,
                        TimeZone = schedule_timezone,
                        SalesLeadId = sl.Id,
                        SalesLeadName = sl.CustomerName,
                        MemberNumber = cMem.MemberNumber,
                        Done = event_done,
                    };
                    db.Calendar_Event.Add(ap);
                    if (Type == "Event")
                    {
                        message = "Create appoiment success !";
                    }
                    else
                    {
                        message = "Create log success !";
                    }
                }
                else
                {
                    ap = db.Calendar_Event.Find(appointment_id);
                    if (ap == null)
                    {
                        throw new Exception(nameof(Type) + " not found!");
                    }
                    ap.Name = title;
                    ap.Description = description;
                    ap.StartEvent = start;
                    ap.GMT = gmt;
                    ap.TimeZone = schedule_timezone;
                    ap.LastUpdateBy = cMem.FullName;
                    ap.LastUpdateAt = DateTime.UtcNow;
                    ap.Done = event_done;
                    db.Entry(ap).State = EntityState.Modified;
                    if (Type == "Event")
                    {
                        message = "Update appoiment success !";
                    }
                    else
                    {
                        message = " Update log success !";
                    }
                }
                if (Type != "Event")
                {
                    var update_lead = db.C_SalesLead.Where(l => l.Id == ap.SalesLeadId).FirstOrDefault();
                    update_lead.LastNoteAt = DateTime.UtcNow;
                    update_lead.LastNote = description;
                    update_lead.CallOfNumber = CallOfNumber;
                    if (StatusInteraction != null)
                    {
                        var stt = db.C_SalesLead_Interaction_Status.Find(StatusInteraction);
                        update_lead.Interaction_Status_Id = stt.Id;
                        update_lead.Interaction_Status = stt.Name;
                    }
                    update_lead.UpdateBy = cMem.FullName;
                    update_lead.UpdateAt = DateTime.UtcNow;
                    db.Entry(update_lead).State = EntityState.Modified;
                }
                db.SaveChanges();
                return Json(new object[] { true, message });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });
            }
        }
        public JsonResult getAppoimentInfo(string appointment_id)
        {
            try
            {
                var ap = db.Calendar_Event.Find(appointment_id);
                if (ap == null)
                {
                    throw new Exception("Appointment not found!");
                }

                return Json(new object[] { true, ap });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });
            }
        }
        public JsonResult getLogInfo(string appointment_id)
        {
            try
            {
                var ap = db.Calendar_Event.Find(appointment_id);
                if (ap == null)
                {
                    throw new Exception("Log not found!");
                }
                return Json(new object[] { true, ap });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });
            }
        }
        public ActionResult getPartialLead(long? Id)
        {
            C_Customer cus = null;
            C_SalesLead lead = null;
            if (Id != null)
            {
                cus = db.C_Customer.Find(Id);
                lead = db.C_SalesLead.Find(Id);
            }
            return PartialView("_Partial_LeadPopup", (cus, lead));
        }
        private void CheckExistSalon(C_Customer c)
        {
            using (WebDataModel _db = new WebDataModel())
            {
                //if (_db.C_Customer.Any(m =>
                //    !string.IsNullOrEmpty(c.BusinessName) && m.BusinessName == c.BusinessName && m.Id != c.Id ||
                //    !string.IsNullOrEmpty(c.BusinessPhone) && m.BusinessPhone == c.BusinessPhone && m.Id != c.Id)
                //)
                //{
                //    throw new Exception("Business Name or Business Number already exists!");
                //}

                if (_db.C_Customer.Any(m => !string.IsNullOrEmpty(c.BusinessEmail) && m.BusinessEmail == c.BusinessEmail && m.Id != c.Id))
                {
                    throw new Exception("Your Salon email already exists!");
                }
                if (_db.C_Customer.Any(m => !string.IsNullOrEmpty(c.CellPhone) && m.CellPhone == c.CellPhone && m.Id != c.Id))
                {
                    throw new Exception("Contact phone already exists!");
                }
                if (_db.C_Customer.Any(m => !string.IsNullOrEmpty(c.SalonPhone) && m.SalonPhone == c.SalonPhone && m.Id != c.Id))
                {
                    throw new Exception("Your Salon phone already exists!");
                }
            }
        }
        public async Task<JsonResult> SaveLead(C_Customer c, List<string> feature_interes)
        {
            try
            {
                if (c.Id > 0)
                {
                    var cus = db.C_Customer.Find(c.Id);
                    if (cus == null)
                    {
                        throw new Exception("Lead not found!");
                    }
                    if (db.C_Customer.Any(o => o.SalonEmail == c.SalonEmail && c.Id != o.Id))
                    {
                        throw new Exception("Salon Email already exist.");
                    }
                    if (!string.IsNullOrEmpty(c.SalonPhone) && db.C_Customer.Any(o => o.SalonPhone == c.SalonPhone && c.Id != o.Id))
                    {
                        throw new Exception("Salon Phone already exist.");
                    }

                    cus.ContactName = cus.OwnerName = c.ContactName;
                    cus.CellPhone = cus.OwnerMobile = c.CellPhone;
                    cus.Email = c.Email;
                    cus.BusinessName = c.BusinessName;
                    cus.BusinessCountry = c.BusinessCountry;
                    cus.BusinessEmail = cus.SalonEmail = c.BusinessEmail;
                    cus.BusinessAddressStreet = cus.SalonAddress1 = c.BusinessAddressStreet;
                    cus.BusinessCity = cus.SalonCity = c.BusinessCity;
                    cus.BusinessPhone = cus.SalonPhone = c.BusinessPhone;
                    cus.BusinessState = cus.SalonState = c.BusinessState;
                    cus.BusinessZipCode = cus.SalonZipcode = c.BusinessZipCode;
                    cus.SalonTimeZone = c.SalonTimeZone;

                    if (cMem.SiteId > 1)
                    {
                        cus.PartnerCode = cMem.BelongToPartner;
                    }
                    c.SiteId = cMem.SiteId;
                    if (cus.SalonTimeZone != c.SalonTimeZone)
                    {
                        var listTimeZone = new MerchantService().ListTimeZone();
                        var listIMSTimeZone = new string[] { "Eastern", "Central", "Mountain", "Pacific", "VietNam" };
                        if (listIMSTimeZone.Any(t => t.Contains(c.SalonTimeZone)))
                        {
                            cus.SalonTimeZone_Number = Ext.EnumParse<TIMEZONE_NUMBER>(c.SalonTimeZone).Text();
                        }
                        else
                        {
                            cus.SalonTimeZone_Number = listTimeZone.FirstOrDefault(t => t.Name == c.SalonTimeZone)?.TimeDT;
                        }
                    }
                    cus.UpdateBy = cMem.FullName;
                    if (!string.IsNullOrWhiteSpace(Request["expected_open_date"]))
                    {
                        c.BusinessStartDate = DateTime.Parse(Request["expected_open_date"].ToString());
                    }
                    db.Entry(cus).State = EntityState.Modified;

                    var lead = db.C_SalesLead.Find(c.Id);
                    if (lead != null)
                    {
                        //lead.SL_Status = int.Parse(Request["status"]);
                        //lead.SL_StatusName = db.C_SalesLead_Status.Find(lead.SL_Status)?.Name;
                        lead.PotentialRateScore = int.Parse(Request["rate"] ?? "0");
                        lead.Features_Interes = string.Join(",", feature_interes ?? new List<string>());
                        lead.Features_Interes_other = Request["feature_interes_other"] == "true" ? Request["c_other_note"] : "";
                        lead.UpdateAt = DateTime.UtcNow;
                        lead.UpdateBy = cMem.FullName;
                        db.Entry(lead).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                }
                else
                {
                    CheckExistSalon(c);
                    c.Id = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssfff"));
                    /*
                    var num_cus = db.C_Customer.Where(m => m.CreateAt.Value.Year == DateTime.Now.Year && m.CreateAt.Value.Month == DateTime.Now.Month).Count();
                    c.CustomerCode = DateTime.Now.ToString("yyMM") + (num_cus + 1).ToString().PadLeft(4, '0') + DateTime.Now.ToString("ff");
                    */
                    c.CustomerCode = new MerchantService().MakeId();
                    //c.BusinessState = db.Ad_USAState.Where(s => s.name.ToLower() == c.BusinessState.ToLower()).FirstOrDefault()?.abbreviation;
                    c.OwnerName = c.ContactName;
                    c.OwnerMobile = c.CellPhone;
                    c.CreateAt = DateTime.UtcNow;
                    c.CreateBy = cMem.FullName;
                    c.Password = db.SystemConfigurations.FirstOrDefault().MerchantPasswordDefault ?? string.Empty;
                    c.MD5PassWord = SecurityLibrary.Md5Encrypt(c.Password);

                    c.SalonEmail = c.BusinessEmail;
                    c.SalonAddress1 = c.BusinessAddressStreet;
                    c.SalonCity = c.BusinessCity;
                    c.SalonPhone = c.BusinessPhone;
                    c.SalonState = c.BusinessState;
                    c.SalonZipcode = c.BusinessZipCode;

                    if (cMem.SiteId > 1)
                    {
                        c.PartnerCode = cMem.BelongToPartner;
                    }
                    c.SiteId = cMem.SiteId;
                    var listTimeZone = new MerchantService().ListTimeZone();
                    if (c.SalonTimeZone == "Eastern" || c.SalonTimeZone == "Central" || c.SalonTimeZone == "Mountain" || c.SalonTimeZone == "Pacific" || c.SalonTimeZone == "VietNam")
                    {
                        c.SalonTimeZone_Number = Ext.EnumParse<TIMEZONE_NUMBER>(c.SalonTimeZone).Text();
                    }
                    else
                    {
                        c.SalonTimeZone_Number = listTimeZone.FirstOrDefault(t => t.Name == c.SalonTimeZone)?.TimeDT;
                    }
                    c.Active = 1;

                    if (!string.IsNullOrWhiteSpace(Request["expected_open_date"]))
                    {
                        c.BusinessStartDate = DateTime.Parse(Request["expected_open_date"].ToString());
                    }

                    db.C_Customer.Add(c);
                    await db.SaveChangesAsync();
                    await EngineContext.Current.Resolve<IEnrichUniversalService>().InitialStoreDataAsync(c.StoreCode);

                    var lead = new C_SalesLead
                    {
                        Id = Guid.NewGuid().ToString(),
                        CustomerCode = c.CustomerCode,
                        CustomerName = c.BusinessName,
                        MemberNumber = cMem.MemberNumber,
                        SL_Status = LeadStatus.Lead.Code<int>(),
                        SL_StatusName = LeadStatus.Lead.Text(),
                        PotentialRateScore = int.Parse(Request["rate"] ?? "0"),
                        Features_Interes = string.Join(",", feature_interes ?? new List<string>()),
                        Features_Interes_other = Request["feature_interes_other"] == "true" ? Request["c_other_note"] : "",
                        CreateAt = DateTime.UtcNow,
                        CreateBy = cMem.FullName,
                        UpdateAt = DateTime.UtcNow,
                        UpdateBy = cMem.FullName,
                    };

                    db.C_SalesLead.Add(lead);
                    db.SaveChanges();
                    if (Request["appointment"] == "1")
                    {
                        string gmt = Request["schedule_timezone"];
                        gmt = gmt.Substring(4, 6);
                        int hour = int.Parse(Request["schedule_hours"].PadLeft(2, '0'));
                        if (Request["schedule_am_pm"] == "PM")
                        {
                            hour += 12;
                            hour = hour == 24 ? 0 : hour;
                        }
                        string start = DateTime.Parse(Request["schedule_date"]).ToString("yyyy-MM-ddT") + hour.ToString().PadLeft(2, '0') + ":" + Request["schedule_minute"].PadLeft(2, '0') + ":00" + gmt;
                        var calendarEve = new Calendar_Event
                        {
                            Description = Request["appoiment_description"],
                            Color = "#FF8C00",
                            StartEvent = start,
                            GMT = gmt,
                            Name = Request["appoiment_title"],
                            TimeZone = Request["schedule_timezone"],
                            LastUpdateAt = DateTime.UtcNow,
                            LastUpdateBy = cMem.FullName,
                            LastUpdateByNumber = cMem.MemberNumber,
                            CreateAt = DateTime.UtcNow,
                            CreateBy = cMem.FullName,
                            CustomerCode = c.CustomerCode,
                            CustomerName = c.BusinessName,
                            MemberNumber = cMem.MemberNumber,
                            Done = Request["event_done"].ToString() == "1" ? 1 : 0,
                        };
                        var ce = new CalendarViewService(db);
                        ce.AddNewEvent(calendarEve, out string err);
                    }
                }

                return Json(new object[] { true, c, "Lead has been saved!" });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, null, e.Message });
            }

        }
        public JsonResult UpdateStatus(string cus_code, int status_id)
        {
            try
            {
                var lead = db.C_SalesLead.Where(s => s.CustomerCode == cus_code).FirstOrDefault();
                var status = db.C_SalesLead_Status.Find(status_id);
                if (lead == null)
                {
                    throw new Exception("Lead not found!");
                }
                if (status == null)
                {
                    throw new Exception("Lead status not found!");
                }
                lead.SL_Status = status.Id;
                lead.SL_StatusName = status.Name;
                db.Entry(lead).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new object[] { true, status });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });
            }


        }

        public ActionResult Import()
        {
            return View();
        }
        public ActionResult ImportFile()
        {
            try
            {
                HttpPostedFileBase file = Request.Files[0];

                var ftp_host = ConfigurationManager.AppSettings["ftp_host"];
                var ftp_user = ConfigurationManager.AppSettings["ftp_user"];
                var ftp_password = ConfigurationManager.AppSettings["ftp_password"];

                int fileSize = file.ContentLength;
                string fileName = file.FileName;

                //load file truc tiep tu fpt
                //string filepath = $"ftp://{ftp_user}:{ftp_password}@{ftp_host.Replace("ftp://", "")}/{fileName}";
                string path = Server.MapPath("/Upload/Sales_Lead_Data/" + fileName);

                if (System.IO.File.Exists(path))
                {
                    throw new Exception("File is exists.");
                }

                string mimeType = file.ContentType;
                System.IO.Stream fileContent = file.InputStream;
                byte[] fileBytes = fileContent.ReadAsBytes();
                //upload file by ftp
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftp_host + fileName);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = new NetworkCredential(ftp_user, ftp_password);
                request.ContentLength = fileContent.Length;

                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(fileBytes, 0, fileBytes.Length);
                }
                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    var mess = response.StatusDescription;
                }

                int rows = 0;
                int cols = 0;
                ISheet dataCells;
                FtpWebRequest requestD = (FtpWebRequest)WebRequest.Create(ftp_host + fileName);
                requestD.Method = WebRequestMethods.Ftp.DownloadFile;
                requestD.Credentials = new NetworkCredential(ftp_user, ftp_password);
                using (Stream ftpStream = requestD.GetResponse().GetResponseStream())
                using (Stream fileStream = System.IO.File.Create(path))
                {
                    ftpStream.CopyTo(fileStream);
                    fileStream.Position = 0;
                    IWorkbook workbook = new XSSFWorkbook(fileStream);
                    ISheet sheet = workbook.GetSheetAt(0);

                    rows = sheet.LastRowNum;
                    cols = sheet.GetRow(1).LastCellNum;
                    dataCells = sheet;

                    workbook.Close();
                    fileStream.Close();
                    ftpStream.Close();
                }

                Session.Remove("dataCells");
                Session.Remove("Rows");
                Session.Remove("Cols");
                Session["dataCells"] = dataCells;
                Session["Rows"] = rows;
                Session["Cols"] = cols;

                List<string> columnName = new List<string>();
                for (int i = 0; i < cols; i++)
                {
                    columnName.Add(dataCells.GetRow(0).GetCell(i)?.ToString());
                }

                var columnData = new List<List<string>>();

                for (int i = 1; i < (rows > 51 ? 51 : rows); i++)
                {
                    var x = new List<string>();
                    for (int j = 0; j < cols; j++)
                    {
                        dataCells.GetRow(i).GetCell(j)?.SetCellType(CellType.String);
                        var cell = dataCells.GetRow(i).GetCell(j)?.ToString() ?? string.Empty;
                        x.Add(cell);
                    }
                    columnData.Add(x);
                }

                if (!string.IsNullOrEmpty(cMem.DepartmentId))
                {
                    var salesDepartment = db.P_Department.Where(c => "SALES".Equals(c.Type) && cMem.DepartmentId.Contains(c.Id.ToString())).ToList();
                    ViewBag.IsSaler = salesDepartment.Count > 0 ? 1 : 0;
                }

                TempData["ColumnName"] = columnName;
                TempData["ColumnData"] = columnData;
                TempData["TotalRecords"] = rows;
                TempData["Message"] = "Upload file success.";
                TempData["Status"] = "success";
                return PartialView("_Partial_ImportSalesLead");
            }
            catch (Exception e)
            {
                TempData["ColumnName"] = new List<string>();
                TempData["ColumnData"] = new List<List<string>>();
                TempData["TotalRecords"] = 0;
                TempData["Message"] = "Message: Upload file fail, please try later. " + e.Message;
                TempData["Status"] = "warning";
                return PartialView("_Partial_ImportSalesLead");
            }
        }
        public JsonResult ImportSaleLead(string key, int startNumber, string memberNumber, long teamNumber, int countNumber = 500)
        {
            try
            {
                var arrKey = key.Split('|');
                ISheet dataCells = (ISheet)Session["dataCells"];
                int rows = (int)Session["Rows"];
                int cols = (int)Session["Cols"];
                int nextNumber = -1;
                if (startNumber > rows)
                    return Json(new object[] { true, nextNumber }, JsonRequestBehavior.AllowGet);
                if (startNumber + countNumber < rows)
                    nextNumber = startNumber + countNumber;
                else
                {
                    countNumber = rows - startNumber;
                    nextNumber = rows;
                }

                int count = 0;
                var sales = new List<C_SalesLead>();

                string MemberNumber = null;
                string MemberName = null;
                long? TeamNumber = null;
                string TeamName = null;
                if (!string.IsNullOrEmpty(memberNumber))
                {
                    var member = db.P_Member.Where(x => x.MemberNumber == memberNumber).FirstOrDefault();
                    if (member != null)
                    {
                        MemberNumber = member.MemberNumber;
                        MemberName = member.FullName;
                    }
                }
                if (teamNumber != 0)
                {
                    var team = db.P_Department.Where(x => x.Id == teamNumber).FirstOrDefault();
                    if (team != null)
                    {
                        TeamNumber = team.Id;
                        TeamName = team.Name;
                    }
                }

                for (int i = startNumber; i < nextNumber; i++, count++)
                {
                    var sale = new C_SalesLead();
                    string temp = string.Empty;
                    sale.Id = Guid.NewGuid().ToString();
                    sale.SL_Status = LeadStatus.Lead.Code<int>();
                    sale.SL_StatusName = LeadStatus.Lead.Text();
                    sale.L_Type = LeadType.ImportData.Text();
                    sale.CreateAt = DateTime.UtcNow;
                    sale.CreateBy = cMem.FullName;
                    sale.CreateByMemberNumber = cMem.MemberNumber;
                    sale.L_IsVerify = false;
                    sale.L_IsSendMail = false;
                    sale.MemberNumber = MemberNumber;
                    sale.MemberName = MemberName;
                    sale.TeamNumber = TeamNumber;
                    sale.TeamName = TeamName;
                    int location = -1;
                    location = Array.IndexOf(arrKey, "0");
                    if (!(location < 0))
                    {
                        dataCells.GetRow(i + 1).GetCell(location)?.SetCellType(CellType.String);
                        var cell = dataCells.GetRow(i + 1).GetCell(location);
                        sale.CustomerName = cell?.ToString() ?? string.Empty;
                        sale.L_SalonName = cell?.ToString() ?? string.Empty;
                    }
                    location = Array.IndexOf(arrKey, "1");
                    if (!(location < 0))
                    {
                        dataCells.GetRow(i + 1).GetCell(location)?.SetCellType(CellType.String);
                        var cell = dataCells.GetRow(i + 1).GetCell(location);
                        sale.L_ContactPhone = cell?.ToString() ?? string.Empty;
                        sale.L_Phone = cell?.ToString() ?? string.Empty;
                    }
                    location = Array.IndexOf(arrKey, "2");
                    if (!(location < 0))
                    {
                        dataCells.GetRow(i + 1).GetCell(location)?.SetCellType(CellType.String);
                        var cell = dataCells.GetRow(i + 1).GetCell(location);
                        temp = cell?.ToString() ?? string.Empty;
                        sale.L_Address = temp.Length > 199 ? temp.Substring(0, 199) : temp;
                    }
                    location = Array.IndexOf(arrKey, "3");
                    if (!(location < 0))
                    {
                        dataCells.GetRow(i + 1).GetCell(location)?.SetCellType(CellType.String);
                        var cell = dataCells.GetRow(i + 1).GetCell(location);
                        temp = cell?.ToString() ?? string.Empty;
                        sale.L_City = temp.Length > 49 ? temp.Substring(0, 49) : temp;
                    }
                    location = Array.IndexOf(arrKey, "4");
                    if (!(location < 0))
                    {
                        dataCells.GetRow(i + 1).GetCell(location)?.SetCellType(CellType.String);
                        var cell = dataCells.GetRow(i + 1).GetCell(location);
                        temp = cell?.ToString() ?? string.Empty;
                        sale.L_State = temp.Length > 49 ? temp.Substring(0, 49) : temp;
                    }
                    location = Array.IndexOf(arrKey, "5");
                    if (!(location < 0))
                    {
                        dataCells.GetRow(i + 1).GetCell(location)?.SetCellType(CellType.String);
                        var cell = dataCells.GetRow(i + 1).GetCell(location);
                        temp = cell?.ToString() ?? string.Empty;
                        sale.L_Country = temp.Length > 49 ? temp.Substring(0, 49) : temp;
                    }
                    location = Array.IndexOf(arrKey, "6");
                    if (!(location < 0))
                    {
                        dataCells.GetRow(i + 1).GetCell(location)?.SetCellType(CellType.String);
                        var cell = dataCells.GetRow(i + 1).GetCell(location);
                        temp = cell?.ToString() ?? string.Empty;
                        sale.L_Zipcode = temp.Length > 49 ? temp.Substring(0, 49) : temp;
                    }
                    location = Array.IndexOf(arrKey, "7");
                    if (!(location < 0))
                    {
                        dataCells.GetRow(i + 1).GetCell(location)?.SetCellType(CellType.String);
                        var cell = dataCells.GetRow(i + 1).GetCell(location);
                        temp = cell?.ToString() ?? string.Empty;

                        var end = temp.IndexOf(',');
                        if (end == -1)
                            sale.L_City = temp.Length > 49 ? temp.Substring(0, 49) : temp;
                        else
                        {
                            sale.L_City = end > 49 ? temp.Substring(0, 49) : temp.Substring(0, end);
                            temp = temp.Remove(0, end + 1);
                            if (temp.Length > 5)
                            {
                                var start = temp.Length - 5;
                                sale.L_Zipcode = temp.Substring(start, 5);
                                sale.L_State = temp.Remove(start, 5).TrimStart().TrimEnd();
                            }
                            else
                            {
                                sale.L_State = temp.Remove(0, temp.Length).TrimStart().TrimEnd();
                            }
                        }
                    }
                    location = Array.IndexOf(arrKey, "8");
                    if (!(location < 0))
                    {
                        dataCells.GetRow(i + 1).GetCell(location)?.SetCellType(CellType.Numeric);
                        var cell = dataCells.GetRow(i + 1).GetCell(location)?.NumericCellValue;
                        sale.PotentialRateScore = decimal.ToInt32(Convert.ToDecimal(cell)) > 5 ? 0 : decimal.ToInt32(Convert.ToDecimal(cell));
                    }
                    location = Array.IndexOf(arrKey, "9");
                    if (!(location < 0))
                    {
                        dataCells.GetRow(i + 1).GetCell(location)?.SetCellType(CellType.String);
                        var cell = dataCells.GetRow(i + 1).GetCell(location);
                        sale.MemberNumber = cell?.ToString() ?? sale.MemberNumber;
                    }
                    location = Array.IndexOf(arrKey, "10");
                    if (!(location < 0))
                    {
                        dataCells.GetRow(i + 1).GetCell(location)?.SetCellType(CellType.String);
                        var cell = dataCells.GetRow(i + 1).GetCell(location);
                        sale.MemberName = cell?.ToString() ?? sale.MemberName;
                    }
                    location = Array.IndexOf(arrKey, "11");
                    if (!(location < 0))
                    {
                        var cell = dataCells.GetRow(i + 1).GetCell(location);
                        string moreInfo = "{";
                        var indexes = arrKey.Select((v, z) => new
                        {
                            value = v,
                            index = z
                        }).Where(pair => pair.value == "11").Select(pair => pair.index).ToArray();
                        indexes.ForEach(ind =>
                        {
                            dataCells.GetRow(i + 1).GetCell(ind)?.SetCellType(CellType.String);
                            moreInfo += "\"" + dataCells.GetRow(i + 1).GetCell(ind) + "\":\"" + dataCells.GetRow(i + 1).GetCell(ind) + "\",";
                        });
                        moreInfo += "}";
                        sale.L_MoreInfo = moreInfo;
                    }
                    sales.Add(sale);
                }
                if (sales != null)
                {
                    db.C_SalesLead.AddRange(sales);
                    db.SaveChanges();
                }

                return Json(new object[] { true, nextNumber, countNumber }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public string SalonAddressView(SaleLeadInfo lead)
        {
            return String.Join(", ", new[]
            {
                lead.SalonAddress1,
                lead.SalonCity,
                lead.SalonState,
                lead.BusinessCountry
            }.Where(entry => (entry?.Trim() ?? "") != ""));
        }
        string ContactAddressView(SaleLeadInfo lead)
        {
            return String.Join(", ", new[]
            {
            lead.ContactAddress,
            lead.City,
            lead.State,
            lead.Country
            }.Where(entry => (entry?.Trim() ?? "") != ""));
        }

        //#region create trial account

        //public async Task<ActionResult> CreateTrialAccount(long Id)
        //{
        //    try
        //    {
        //        var _registerMangoService = new SalesLeadService();
        //        var customer = db.C_Customer.Where(x => x.Id == Id).FirstOrDefault();
        //        var sl = db.C_SalesLead.Where(x => x.Id == Id).FirstOrDefault();
        //        customer.WordDetermine = "Trial";
        //        sl.SL_Status = 2;
        //        sl.SL_StatusName = "Trial Account";
        //        await _registerMangoService.SendMailVerify(email: customer.BusinessEmail, name: customer.BusinessName, phone: customer.BusinessPhone, link: WebConfigurationManager.AppSettings["HostRegisterMango"] + "/verifyforemail?key=" + SecurityLibrary.Encrypt(customer.BusinessEmail));
        //        db.SaveChanges();
        //        return Json(new { status = true, message = "Create trial account success" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { status = false, message = ex.Message });
        //    }
        //}

        //#endregion
        public ActionResult ChangeTab(string TabName, FilterDataView DataFilter)
        {
            var sale_dep = db.P_Department.Where(d => d.Type == "SALES" && (cMem.SiteId == 1 || d.SiteId == cMem.SiteId)).Select(d => d.Id).ToList().Select(d => d.ToString());
            var currentDeparmentsUser = db.P_Department.Where(d => d.Type == "SALES" && d.GroupMemberNumber.Contains(cMem.MemberNumber));
            bool ShowSalesPerson = false;
            bool ShowTeam = false;
            if (access.Any(k => k.Key.Equals("sales_lead_view_all")) == true && access["sales_lead_view_all"] == true)
            {
                ViewBag.ListMember = db.P_Member.Where(m => m.Active == true && sale_dep.Any(n => m.DepartmentId.Contains(n))).ToList();
                ShowSalesPerson = true;
                ShowTeam = true;
                var Team = db.P_Department.Where(d => d.Type == "SALES" && d.Active.Value && d.ParentDepartmentId != null);
                if (cMem.SiteId != 1)
                {
                    Team = Team.Where(x => x.SiteId == cMem.SiteId);
                }
                ViewBag.Team = Team.ToList();
            }
            else if (access.Any(k => k.Key.Equals("sales_lead_view_team")) == true && access["sales_lead_view_team"] == true)
            {
                List<string> listMemberNumber = new List<string>();
                var ManagerDep = db.P_Department.Where(x => x.SupervisorNumber == cMem.MemberNumber && x.ParentDepartmentId != null && x.Type == "SALES");
                if (cMem.SiteId != 1)
                {
                    ManagerDep = ManagerDep.Where(x => x.SiteId == cMem.SiteId);
                }
                ViewBag.Team = ManagerDep.ToList();
                if (ManagerDep.Count() > 0)
                {
                    foreach (var dep in ManagerDep)
                    {
                        listMemberNumber.AddRange(dep.GroupMemberNumber.Split(','));
                        ShowSalesPerson = true;
                    }
                    ShowTeam = true;
                }
                if (currentDeparmentsUser.Count() > 0)
                {
                    foreach (var deparment in currentDeparmentsUser)
                    {
                        if (deparment.LeaderNumber == cMem.MemberNumber)
                        {
                            listMemberNumber.AddRange(deparment.GroupMemberNumber.Split(','));
                            ShowSalesPerson = true;
                        }
                    }
                }
                listMemberNumber.Add(cMem.MemberNumber);
                listMemberNumber = listMemberNumber.GroupBy(x => x).Select(x => x.Key).ToList();
                ViewBag.ListMember = (from m in db.P_Member join l in listMemberNumber on m.MemberNumber equals l where (cMem.SiteId == 1 || m.SiteId == cMem.SiteId) select m).ToList();
            }
            ViewBag.ShowSalesPerson = ShowSalesPerson;
            ViewBag.ShowTeam = ShowTeam;
            DateTime From = DateTime.UtcNow.UtcToIMSDateTime().AddMonths(-3);
            ViewBag.From = DataFilter.FromDate ?? new DateTime(From.Year, From.Month, 1).ToString("MM/dd/yyyy");
            ViewBag.To = DataFilter.ToDate ?? DateTime.UtcNow.UtcToIMSDateTime().ToString("MM/dd/yyyy");
            ViewBag.S_Status = DataFilter.Status;
            ViewBag.S_Team = DataFilter.Team;
            ViewBag.S_Member = DataFilter.Member;
            ViewBag.S_SearchText = DataFilter.SearchText;
            ViewBag.S_State = DataFilter.State;
            var siteIds = db.Sites.ToList();
            ViewBag.S_SiteId = siteIds;
            ViewBag.InteractionStatus = db.C_SalesLead_Interaction_Status.Where(x => x.Status == true).OrderBy(x => x.Order).Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name,
            }).ToList();
            switch (TabName)
            {
                //tab sales lead
                case "sales-lead":
                    bool canAccessSalesLead = AppFunc.CanAccess("sales_lead_view_all");
                    string merchantName = LeadStatus.Merchant.Text();
                    string sliceName = LeadStatus.SliceAccount.Text();
                    ViewBag.status = db.C_SalesLead_Status.ToList();
                    ViewBag.canAccessSalesLead = canAccessSalesLead;
                    return PartialView("_Tab_SalesLead");
                // tab report
                case "report":
                    return PartialView("_Tab_Report");
                // tab report
                case "import-data":
                    return PartialView("_Tab_ImportData");
                // tab new register
                default:
                    return PartialView("_Tab_NewRegister");
            }
        }
        public ActionResult GetListMemberAssigned()
        {
            var sale_dep = db.P_Department.Where(d => d.Type == "SALES").Select(d => d.Id).ToList().Select(d => d.ToString());
            var currentDeparmentsUser = db.P_Department.Where(d => d.Type == "SALES" && d.GroupMemberNumber.Contains(cMem.MemberNumber));
            if (access.Any(k => k.Key.Equals("sales_lead_view_all")) == true && access["sales_lead_view_all"] == true)
            {
                var ListMember = db.P_Member.Where(m => m.Active == true && sale_dep.Any(n => m.DepartmentId.Contains(n))).Select(x => new SelectListItem
                {
                    Value = x.MemberNumber,
                    Text = x.FullName + " - #" + x.MemberNumber,
                }).ToList();
                return Json(new { status = true, data = ListMember }, JsonRequestBehavior.AllowGet);
            }
            else if (access.Any(k => k.Key.Equals("sales_lead_view_team")) == true && access["sales_lead_view_team"] == true)
            {
                List<string> listMemberNumber = new List<string>();
                var ManagerDep = db.P_Department.Where(x => x.SupervisorNumber == cMem.MemberNumber && x.ParentDepartmentId != null && x.Type == "SALES").ToList();
                if (ManagerDep.Count() > 0)
                {
                    foreach (var dep in ManagerDep)
                    {
                        listMemberNumber.AddRange(dep.GroupMemberNumber.Split(','));
                    }
                }
                if (currentDeparmentsUser.Count() > 0)
                {
                    foreach (var deparment in currentDeparmentsUser)
                    {
                        if (deparment.LeaderNumber == cMem.MemberNumber)
                        {
                            listMemberNumber.AddRange(deparment.GroupMemberNumber.Split(','));
                        }
                    }
                }
                listMemberNumber.Add(cMem.MemberNumber);
                listMemberNumber = listMemberNumber.GroupBy(x => x).Select(x => x.Key).ToList();
                var ListMember = (from m in db.P_Member join l in listMemberNumber on m.MemberNumber equals l select m).Select(x => new SelectListItem
                {
                    Value = x.MemberNumber,
                    Text = x.FullName + " - #" + x.MemberNumber,
                }).ToList();
                return Json(new { status = true, data = ListMember }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { status = false }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetListTeamAssigned()
        {
            if (access.Any(k => k.Key.Equals("sales_lead_view_all")) == true && access["sales_lead_view_all"] == true)
            {
                var ListTeam = db.P_Department.Where(d => d.Type == "SALES" && d.ParentDepartmentId != null).Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.Name
                }).ToList();
                return Json(new { status = true, data = ListTeam }, JsonRequestBehavior.AllowGet);
            }
            else if (access.Any(k => k.Key.Equals("sales_lead_view_team")) == true && access["sales_lead_view_team"] == true)
            {
                List<string> listMemberNumber = new List<string>();
                var ListTeam = db.P_Department.Where(x => x.SupervisorNumber == cMem.MemberNumber && x.ParentDepartmentId != null && x.Type == "SALES").Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.Name
                }).ToList();
                return Json(new { status = true, data = ListTeam }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { status = false }, JsonRequestBehavior.AllowGet);
        }
        #region  Sales Lead Tab
        // load data for datatable
        [HttpPost]
        public ActionResult LoadState_SalesLead()
        {
            int statusMerchant = LeadStatus.Merchant.Code<int>();
            int statusLead = LeadStatus.Lead.Code<int>();
            int statusTrial = LeadStatus.TrialAccount.Code<int>();
            int statusSlice = LeadStatus.SliceAccount.Code<int>();
            var ListState = new List<StateSearchSalesLeadModel>();

            var cusQuery = from cus in db.C_Customer join sl in db.C_SalesLead on cus.CustomerCode equals sl.CustomerCode /*where sl.SL_Status != statusMerchant && (sl.SL_Status != statusSlice || (sl.SL_Status == statusSlice && sl.L_IsVerify != true)) */select new { cus, sl };
            var queryState = from states in db.Ad_USAState select states;
            List<string> listState = new List<string>();
            List<string> listMemberNumber = new List<string>();
            List<long> listTeam = new List<long>();
            bool showOtherState = false;
            int totalRecord = 0;
            //hard code for siteId
            cusQuery = cusQuery.Where(x => x.cus.SiteId == cMem.SiteId);
            if (access.Any(k => k.Key.Equals("sales_lead_view_all")) == true && access["sales_lead_view_all"] == true)
            {
                var query = from state in queryState
                            join cus in cusQuery.GroupBy(x => x.cus.BusinessState) on state.abbreviation.Trim().ToLower() equals cus.Key.Trim().ToLower() into gj
                            from j in gj.DefaultIfEmpty()
                            select new StateSearchSalesLeadModel { Code = state.abbreviation, Name = state.name, Number = j.Key != null ? j.Count() : 0 };
                ListState.AddRange(query.ToList());
                totalRecord = query.Count();
                showOtherState = true;
            }
            else if (access.Any(k => k.Key.Equals("sales_lead_view_team")) == true && access["sales_lead_view_team"] == true)
            {
                var ManagerDep = db.P_Department.Where(x => x.SupervisorNumber == cMem.MemberNumber && x.ParentDepartmentId != null && x.Type == "SALES").ToList();
                if (ManagerDep.Count() > 0)
                {
                    foreach (var dep in ManagerDep)
                    {
                        listState.AddRange(dep.SaleStates.Split(','));
                        listMemberNumber.AddRange(dep.GroupMemberNumber.Split(','));
                    }
                }
                var currentDeparmentsUser = db.P_Department.Where(d => d.Type == "SALES" && d.GroupMemberNumber.Contains(cMem.MemberNumber));
                if (currentDeparmentsUser.Count() > 0)
                {
                    foreach (var deparment in currentDeparmentsUser)
                    {
                        listState.AddRange(deparment.SaleStates.Split(','));
                        listTeam.Add(deparment.Id);
                        if (deparment.LeaderNumber == cMem.MemberNumber)
                        {
                            listMemberNumber.AddRange(deparment.GroupMemberNumber.Split(','));
                        }
                    }
                }
                listMemberNumber.Add(cMem.MemberNumber);
                listMemberNumber = listMemberNumber.GroupBy(x => x).Select(x => x.Key).ToList();
                queryState = queryState.Where(x => listState.Any(y => y.Contains(x.abbreviation)));
                if (listState.Contains("Other"))
                {
                    showOtherState = true;
                }
                var query = from state in queryState
                            join cus in cusQuery.Where(x => listMemberNumber.Any(s => s.Contains(x.sl.MemberNumber) || (string.IsNullOrEmpty(x.sl.MemberNumber) && listTeam.Any(y => y == x.sl.TeamNumber)) || string.IsNullOrEmpty(x.sl.MemberNumber))).GroupBy(x => x.cus.BusinessState) on state.abbreviation.Trim().ToLower() equals cus.Key.Trim().ToLower() into gj
                            from j in gj.DefaultIfEmpty()
                            select new StateSearchSalesLeadModel { Code = state.abbreviation, Name = state.name, Number = j.Key != null ? j.Count() : 0 };
                ListState.AddRange(query.ToList());
                totalRecord = query.Count();
            }
            return Json(new
            {
                showOtherState,
                data = ListState,
                recordsTotal = totalRecord
            });
        }
        public ActionResult SalesLead_LoadList(IDataTablesRequest dataTablesRequest, string State, string Team, string DataFrom, string SalesPerson, string member, DateTime? fdate, DateTime? tdate, string Type, int? SiteId, string SearchText, int? Status)
        {
            string RegisterOnIMSType = LeadType.RegisterOnIMS.Text();
            string CreateBySaler = LeadType.CreateBySaler.Text();
            string SubscribeMangoType = LeadType.SubscribeMango.Text();
            string TrialAccountType = LeadType.TrialAccount.Text();
            string ImportDataType = LeadType.ImportData.Text();
            string SliceAccountDataType = LeadType.SliceAccount.Text();
            int statusMerchant = LeadStatus.Merchant.Code<int>();
            int statusLead = LeadStatus.Lead.Code<int>();
            int statusTrial = LeadStatus.TrialAccount.Code<int>();
            int statusSlice = LeadStatus.SliceAccount.Code<int>();
            var currentDeparmentsUser = db.P_Department.Where(d => d.Type == "SALES" && d.GroupMemberNumber.Contains(cMem.MemberNumber));
            var query = from cus in db.C_Customer
                        join sl in db.C_SalesLead on cus.CustomerCode equals sl.CustomerCode
                        //where sl.SL_Status != statusMerchant && (sl.SL_Status != statusSlice || (sl.SL_Status == statusSlice && sl.L_IsVerify != true))
                        //join cal in db.Calendar_Event on sl.Id equals cal.SalesLeadId  into cals from cal in cals.FirstOrDefault() 
                        let mb = db.P_Member.Where(x => x.MemberNumber == sl.MemberNumber).FirstOrDefault()
                        //let dp = db.P_Department.Where(x=>x.Id == sl.TeamNumber).FirstOrDefault()
                        orderby cus.CreateAt
                        //let nextappoi = db.Calendar_Event.Where(p2 => sl.Id == p2.SalesLeadId && p2.Type == "Event").OrderByDescending(x => x.StartEvent).FirstOrDefault()
                        //join sttsl in db.C_SalesLead_Status on sl.SL_Status equals sttsl.Id
                        select new { sl, mb, cus, };
            //filter data create
            if (!string.IsNullOrEmpty(DataFrom))
            {
                switch (DataFrom)
                {
                    //mango site
                    case "mango-site":
                        query = query.Where(x => x.sl.L_Type == SubscribeMangoType || x.sl.L_Type == TrialAccountType);
                        break;
                    // external data
                    case "external-data":
                        query = query.Where(x => x.sl.L_Type == ImportDataType);
                        break;
                    // from ims
                    default:
                        query = query.Where(x => x.sl.L_Type != ImportDataType && x.sl.L_Type != SubscribeMangoType && x.sl.L_Type != TrialAccountType && x.sl.L_Type != ImportDataType);
                        break;
                }
            }
            //hard code for siteId
            if (cMem.SiteId == 1)
            {
                if (SiteId != null)
                {
                    query = query.Where(x => x.cus.SiteId == SiteId);
                }
            }
            else
            {
                query = query.Where(x => x.cus.SiteId == cMem.SiteId);
            }
            //filter data  from date
            if (fdate != null)
            {
                var From = (fdate.Value.Date + new TimeSpan(0, 0, 0)).IMSToUTCDateTime();
                query = query.Where(x => x.cus.CreateAt >= From);
            }
            //filter data to date
            if (tdate != null)
            {
                var To = (tdate.Value.Date + new TimeSpan(23, 59, 59)).IMSToUTCDateTime(); ;
                query = query.Where(x => x.cus.CreateAt <= To);
            }
            //filter data to type
            if (Type != null)
            {
                query = query.Where(x => Type.IndexOf(x.sl.SL_Status.ToString()) >= 0);
            }
            //filter data to status
            if (Status != null)
            {
                query = query.Where(x => x.sl.Interaction_Status_Id == Status);
            }

            //filter data state
            if (!string.IsNullOrEmpty(State))
            {
                if (State.Contains("Other"))
                {
                    var ListAllState = db.Ad_USAState.Select(x => x.abbreviation).ToArray();
                    query = query.Where(x => State.Contains(x.sl.L_State) || !ListAllState.Contains(x.sl.L_State) || string.IsNullOrEmpty(x.sl.L_State));
                }
                else
                {
                    query = query.Where(x => State.Contains(x.sl.L_State));
                }
            }
            //filter data sales person
            if (!string.IsNullOrEmpty(SalesPerson))
            {
                if (SalesPerson == "Unassigned")
                {
                    query = query.Where(x => string.IsNullOrEmpty(x.sl.MemberNumber));
                }
                else
                {
                    query = query.Where(x => x.sl.MemberNumber == SalesPerson);
                }
            }
            //filter data state
            if (!string.IsNullOrEmpty(Team))
            {
                List<string> listMemberNumber = new List<string>();
                List<string> listState = new List<string>();
                long TeamId = long.Parse(Team);
                var TeamFilter = db.P_Department.Find(long.Parse(Team));

                if (TeamFilter.SaleStates != null)
                    listState.AddRange(TeamFilter.SaleStates.Split(','));
                listMemberNumber.AddRange(TeamFilter.GroupMemberNumber.Split(','));
                if (listState.Count > 0)
                {
                    if (listState.Contains("Other"))
                    {
                        var ListAllState = db.Ad_USAState.Select(x => x.abbreviation).ToArray();
                        query = query.Where(x => listMemberNumber.Any(y => y.Contains(x.sl.MemberNumber)) || (string.IsNullOrEmpty(x.sl.MemberNumber) && x.sl.TeamNumber == TeamId) || ((!ListAllState.Any(y => y.Contains(x.sl.L_State)) || string.IsNullOrEmpty(x.sl.L_State) || listState.Any(s => s.Contains(x.cus.BusinessState))) && string.IsNullOrEmpty(x.sl.MemberNumber) && x.sl.TeamNumber == null));
                    }
                    else
                    {
                        query = query.Where(x => listMemberNumber.Any(y => y.Contains(x.sl.MemberNumber)) || (string.IsNullOrEmpty(x.sl.MemberNumber) && x.sl.TeamNumber == TeamId) || (listState.Any(s => s.Contains(x.cus.BusinessState)) && string.IsNullOrEmpty(x.sl.MemberNumber) && x.sl.TeamNumber == null));
                    }
                }
                else
                {
                    query = query.Where(x => listMemberNumber.Any(y => y.Contains(x.sl.MemberNumber)) || (string.IsNullOrEmpty(x.sl.MemberNumber) && x.sl.TeamNumber == TeamId));
                }
            }
            //view all sales lead if permission is view all
            if (!AppFunc.CanAccess("sales_lead_view_all"))
            {
                if (AppFunc.CanAccess("sales_lead_view_team"))
                {
                    List<string> listMemberNumber = new List<string>();
                    List<string> listState = new List<string>();
                    List<long> listTeam = new List<long>();
                    var ManagerDep = db.P_Department.Where(x => x.SupervisorNumber == cMem.MemberNumber && x.ParentDepartmentId != null && x.Type == "SALES").ToList();
                    if (ManagerDep.Count() > 0)
                    {
                        foreach (var dep in ManagerDep)
                        {
                            listState.AddRange(dep.SaleStates.Split(','));
                            listMemberNumber.AddRange(dep.GroupMemberNumber.Split(','));
                        }
                    }
                    if (currentDeparmentsUser.Count() > 0)
                    {
                        foreach (var deparment in currentDeparmentsUser)
                        {
                            listState.AddRange(deparment.SaleStates.Split(','));
                            listTeam.Add(deparment.Id);
                            if (deparment.LeaderNumber == cMem.MemberNumber)
                            {
                                listMemberNumber.AddRange(deparment.GroupMemberNumber.Split(','));
                            }
                        }
                    }
                    listMemberNumber.Add(cMem.MemberNumber);
                    listMemberNumber = listMemberNumber.GroupBy(x => x).Select(x => x.Key).ToList();
                    listState = listState.GroupBy(x => x).Select(x => x.Key).ToList();
                    if (listState.Count > 0)
                    {
                        if (listState.Contains("Other"))
                        {
                            var ListAllState = db.Ad_USAState.Select(x => x.abbreviation).ToArray();
                            query = query.Where(x => listMemberNumber.Any(y => y.Contains(x.sl.MemberNumber))
                           || (string.IsNullOrEmpty(x.sl.MemberNumber) && listTeam.Any(y => y == (x.sl.TeamNumber)))
                           || ((!ListAllState.Contains(x.sl.L_State) || string.IsNullOrEmpty(x.sl.L_State) || listState.Any(s => s.Contains(x.sl.L_State))) && string.IsNullOrEmpty(x.sl.MemberNumber) && x.sl.TeamNumber == null));
                            //query = from x in query where ((!(from s in db.Ad_USAState select s.abbreviation).Contains(x.sl.L_State)) || string.IsNullOrEmpty(x.sl.L_State) || (listState.Contains(x.sl.L_State)&& string.IsNullOrEmpty(x.sl.MemberNumber) && x.sl.TeamNumber == null)) select x;
                        }
                        else
                        {
                            query = query.Where(x => listMemberNumber.Any(y => y.Contains(x.sl.MemberNumber)) || x.sl.CreateByMemberNumber == cMem.MemberNumber || (string.IsNullOrEmpty(x.sl.MemberNumber) && listTeam.Any(y => y == (x.sl.TeamNumber))) || (listState.Any(s => s.Contains(x.sl.L_State)) && string.IsNullOrEmpty(x.sl.MemberNumber) && x.sl.TeamNumber == null));
                        }
                    }
                    else
                    {
                        query = query.Where(x => listMemberNumber.Any(y => y.Contains(x.sl.MemberNumber)) || x.sl.CreateByMemberNumber == cMem.MemberNumber || (string.IsNullOrEmpty(x.sl.MemberNumber) && listTeam.Any(y => y == (x.sl.TeamNumber))));
                    }
                }
            }
            //filter data search text
            if (!string.IsNullOrEmpty(SearchText))
            {
                query = query.Where(x => x.sl.L_SalonName.Contains(SearchText.Trim()) || x.sl.L_ContactName.Contains(SearchText.Trim()) || x.sl.L_ContactPhone.Contains(SearchText.Trim()) || x.sl.L_Phone.Contains(SearchText.Trim()) || (x.sl.L_Email.Contains(SearchText.Trim())));
                //query = query.Where(x => x.cus.BusinessName.Contains(searchText.Trim()));
            }
            //if (!string.IsNullOrEmpty(Name))
            //{
            //    query = query.Where(x => x.cus.ContactName.Trim().ToLower().Contains(Name.Trim().ToLower()) || x.cus.BusinessName.Trim().ToLower().Contains(Name.Trim().ToLower()));
            //}
            //if (!string.IsNullOrEmpty(Phone))
            //{
            //    query = query.Where(x => x.sl.L_Phone.Contains(Phone.Trim()) || x.sl.L_ContactPhone.Contains(Phone.Trim()));
            //}
            //if (!string.IsNullOrEmpty(Email))
            //{
            //    query = query.Where(x => x.cus.Email.Contains(Email.Trim()));
            //}
            var totalRecord = query.Count();
            var totalLead = query.Where(x => x.sl.SL_Status == statusLead).Count();
            var totalTrial = query.Where(x => x.sl.SL_Status == statusTrial).Count();
            var totalSlice = query.Where(x => x.sl.SL_Status == statusSlice).Count();
            var totalMerchant = query.Where(x => x.sl.SL_Status == statusMerchant).Count();
            var colSearch = dataTablesRequest.Columns.Where(c => c.Sort != null).FirstOrDefault();
            switch (colSearch.Name)
            {
                case "UpdateAt":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        query = query.OrderBy(s => s.sl.UpdateAt);
                    }
                    else
                    {
                        query = query.OrderByDescending(s => s.sl.UpdateAt);
                    }

                    break;
                case "LastNote":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        query = query.OrderBy(s => s.sl.LastNoteAt);
                    }
                    else
                    {
                        query = query.OrderByDescending(s => s.sl.LastNoteAt);
                    }

                    break;
                case "State":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        query = query.OrderBy(s => s.cus.BusinessState);
                    }
                    else
                    {
                        query = query.OrderByDescending(s => s.cus.BusinessState);
                    }
                    break;
                case "PotentialRateScore":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        query = query.OrderBy(s => s.sl.PotentialRateScore);
                    }
                    else
                    {
                        query = query.OrderByDescending(s => s.sl.PotentialRateScore);
                    }
                    break;
                case "SL_Status":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        query = query.OrderBy(s => s.sl.SL_Status);
                    }
                    else
                    {
                        query = query.OrderByDescending(s => s.sl.SL_Status);
                    }
                    break;
                case "StatusInteraction":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        query = query.OrderBy(s => s.sl.Interaction_Status);
                    }
                    else
                    {
                        query = query.OrderByDescending(s => s.sl.Interaction_Status);
                    }
                    break;
                case "ReferralSource":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        query = query.OrderBy(s => s.sl.ReferralSource);
                    }
                    else
                    {
                        query = query.OrderByDescending(s => s.sl.ReferralSource);
                    }
                    break;
                default:
                    query = query.OrderByDescending(s => s.sl.UpdateAt);
                    break;
            }
            Session["sale_filted"] = new sale_filted
            {
                sales_person = SalesPerson,
                fdate = fdate,
                tdate = tdate,
                Type = Type,
                SearchText = SearchText,
                Status = Status,
                //name = Name,
                //phone = Phone,
                //email = Email,
                datafrom = DataFrom,
                query = query.Select(x => new sale_filted.Query_data { cus = x.cus, LastLogTitle = "Note", LastLogDescription = x.sl.LastNote, sl = x.sl })
            };
            query = query.Skip(dataTablesRequest.Start).Take(dataTablesRequest.Length);//.SortAndPage(dataTablesRequest);
            var data = query.ToList();
            var ViewData = data.Select(x => new
            {
                SalesLeadId = x.sl.Id,
                CustomerCode = x.cus.CustomerCode,
                CustomerName = x.cus.BusinessName,
                ContactName = x.cus.ContactName,
                State = x.cus.BusinessState,
                LastNoteDescription = x.sl.LastNote ?? string.Empty,
                LastNoteDateTime = x.sl.LastNoteAt != null ? x.sl.LastNoteAt.Value.UtcToIMSDateTime().ToString("MMM dd, yyyy hh:mm tt") : "",
                //NextAppoiment = x.nextappoi != null ? x.nextappoi.Name ?? string.Empty : string.Empty,
                //NextAppoimentTime = x.nextappoi != null ? string.Format("{0:r}", AppFunc.ParseTimeToUtc(x.nextappoi.StartEvent)) ?? string.Empty : string.Empty,
                PotentialRateScore = x.sl.PotentialRateScore == null || x.sl.PotentialRateScore == 0 ? 1 : x.sl.PotentialRateScore,
                StatusName = x.sl.SL_StatusName,
                StatusId = x.sl.SL_Status,
                //StatusColor = x.sttsl.Color,
                RemainingDays = x.sl.SL_Status != 1 ? GetRemainingDays(x.sl.CustomerCode) : "",
                CreateAt = x.cus.CreateAt.HasValue ? string.Format("{0:r}", x.cus.CreateAt) : string.Empty,
                UpdateAt = x.sl.UpdateAt.HasValue ? x.sl.UpdateAt.Value.UtcToIMSDateTime().ToString("MMM dd, yyyy hh:mm tt") : string.Empty,
                LastUpdateDesc = x.sl.LastUpdateDesc ?? "",
                UpdateBy = x.cus.UpdateBy ?? x.cus.CreateBy,
                AssignedSalesPerson = x.sl.MemberName ?? "",
                AssignedTeam = x.sl?.TeamName ?? "",
                StatusInteraction = x.sl.Interaction_Status ?? "",
                CallOfNumber = x.sl.CallOfNumber ?? "",
                Color = x.mb?.ProfileDefinedColor ?? "#ffffff",
                ReferralSource = x.sl.ReferralSource
            });
            return Json(new
            {
                recordsTotal = totalRecord,
                totalLead = totalLead,
                totalTrial = totalTrial,
                totalSlice = totalSlice,
                totalMerchant = totalMerchant,
                recordsFiltered = totalRecord,
                draw = dataTablesRequest.Draw,
                data = ViewData
            });
        }
        private class sale_filted
        {
            public string sales_person { get; internal set; }
            public Nullable<DateTime> fdate { get; internal set; }
            public Nullable<DateTime> tdate { get; internal set; }
            public int? Status { get; internal set; }
            public string SearchText { get; internal set; }
            public string Type { get; set; }
            //public string email { get; internal set; }
            public string datafrom { get; internal set; }
            public IQueryable<Query_data> query { get; internal set; }


            internal class Query_data
            {
                public string LastLogDescription { get; set; }
                public string LastLogTitle { get; set; }
                public C_Customer cus { get; internal set; }
                public C_SalesLead sl { get; internal set; }
            }
        }
        private class RI_filted
        {
            public string sales_person { get; internal set; }
            public Nullable<DateTime> fdate { get; internal set; }
            public Nullable<DateTime> tdate { get; internal set; }
            public IQueryable<Query_RI_data> query { get; internal set; }
            public string DataFrom { get; internal set; }
            internal class Query_RI_data
            {
                public string LastLogDescription { get; set; }
                public string LastLogTitle { get; set; }
                public C_SalesLead sl { get; internal set; }
            }
        }
        [HttpPost]
        public ActionResult SalesLead_UpdateRowDataTable(string SalesLeadId)
        {
            var query = from cus in db.C_Customer
                        join sl in db.C_SalesLead on cus.CustomerCode equals sl.CustomerCode
                        where sl.Id == SalesLeadId
                        let mb = db.P_Member.Where(n => n.MemberNumber == sl.MemberNumber).FirstOrDefault()
                        //let dp = db.P_Department.Where(n => n.Id == sl.TeamNumber).FirstOrDefault()
                        select new { sl, mb, cus };
            var x = query.FirstOrDefault();

            var ViewData = new
            {
                SalesLeadId = x.sl.Id,
                CustomerCode = x.cus.CustomerCode,
                CustomerName = x.cus.BusinessName,
                ContactName = x.cus.ContactName,
                State = x.cus.BusinessState,
                LastNoteDescription = x.sl.LastNote ?? string.Empty,
                LastNoteDateTime = x.sl.LastNote != null ? string.Format("{0:r}", (x.sl.LastNoteAt)) : "",
                LastUpdateDesc = x.sl.LastUpdateDesc ?? "",
                //NextAppoiment = x.nextappoi != null ? x.nextappoi.Name ?? string.Empty : string.Empty,
                //NextAppoimentTime = x.nextappoi != null ? string.Format("{0:r}", AppFunc.ParseTimeToUtc(x.nextappoi.StartEvent)) ?? string.Empty : string.Empty,
                PotentialRateScore = x.sl.PotentialRateScore == null || x.sl.PotentialRateScore == 0 ? 1 : x.sl.PotentialRateScore,
                StatusName = x.sl.SL_StatusName,
                StatusId = x.sl.SL_Status,
                //StatusColor = x.sttsl.Color,
                RemainingDays = x.sl.SL_Status != 1 ? GetRemainingDays(x.sl.CustomerCode) : "",
                CreateAt = x.cus.CreateAt.HasValue ? string.Format("{0:r}", x.cus.CreateAt) : string.Empty,
                UpdateAt = x.sl.UpdateAt.HasValue ? string.Format("{0:r}", x.sl.UpdateAt.Value) : string.Empty,
                UpdateBy = x.cus.UpdateBy ?? x.cus.CreateBy,
                AssignedSalesPerson = x.sl.MemberName ?? "",
                AssignedTeam = x.sl?.TeamName ?? "",
                StatusInteraction = x.sl.Interaction_Status ?? "",
                CallOfNumber = x.sl.CallOfNumber ?? "",
                Color = x.mb?.ProfileDefinedColor ?? "#ffffff",
            };
            return Json(new
            {
                data = ViewData
            });
        }
        public async Task<FileStreamResult> ExportExcel()
        {
            try
            {
                int statusMerchant = LeadStatus.Merchant.Code<int>();
                var webinfo = db.SystemConfigurations.FirstOrDefault();
                string[] address = webinfo?.CompanyAddress?.Split(new char[] { '|' });
                var sale_filted = Session["sale_filted"] as sale_filted;
                if (sale_filted == null)
                {
                    throw null;
                }
                //view all sales lead if permission is view all
                string webRootPath = "/upload/other/";
                string fileName = @"TempData.xlsx";
                var memoryStream = new MemoryStream();
                // --- Below code would create excel file with dummy data----  
                using (var fs = new FileStream(Server.MapPath(Path.Combine(webRootPath, fileName)), FileMode.Create, FileAccess.Write))
                {

                    IWorkbook workbook = new XSSFWorkbook();
                    //name style
                    IFont font = workbook.CreateFont();
                    font.IsBold = true;
                    font.FontHeightInPoints = 12;
                    ICellStyle style = workbook.CreateCellStyle();
                    style.SetFont(font);
                    //header style
                    IFont font1 = workbook.CreateFont();
                    font1.IsBold = true;
                    font1.Underline = FontUnderlineType.Double;
                    font1.FontHeightInPoints = 13;
                    ICellStyle style1 = workbook.CreateCellStyle();
                    style1.SetFont(font1);
                    IDataFormat dataFormatCustom = workbook.CreateDataFormat();
                    ISheet excelSheet = workbook.CreateSheet("Sales Lead Report");
                    //set column width
                    excelSheet.SetColumnWidth(1, 15 * 256);
                    excelSheet.SetColumnWidth(2, 15 * 256);
                    excelSheet.SetColumnWidth(3, 20 * 256);
                    excelSheet.SetColumnWidth(4, 10 * 256);
                    excelSheet.SetColumnWidth(5, 20 * 256);
                    excelSheet.SetColumnWidth(6, 15 * 256);
                    excelSheet.SetColumnWidth(7, 20 * 256);
                    excelSheet.SetColumnWidth(8, 20 * 256);
                    //reprot info
                    IRow row1 = excelSheet.CreateRow(0);
                    row1.CreateCell(0).SetCellValue(webinfo?.CompanyName);
                    excelSheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 1));
                    IRow row2 = excelSheet.CreateRow(1);
                    row2.CreateCell(0).SetCellValue(address.Length > 0 ? address[0] : "---");
                    excelSheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 1));
                    IRow row3 = excelSheet.CreateRow(2);
                    row3.CreateCell(0).SetCellValue(address.Length > 0 ? address[1] + "," + address[2] + address[3] : "---,--- #####");
                    excelSheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 1));
                    IRow row4 = excelSheet.CreateRow(3);
                    row4.CreateCell(0).SetCellValue("www.enrichcous.com");
                    excelSheet.AddMergedRegion(new CellRangeAddress(3, 3, 0, 1));
                    ICell cell = row1.CreateCell(5);
                    cell.SetCellValue(new XSSFRichTextString("SALES LEAD REPORT"));
                    excelSheet.AddMergedRegion(new CellRangeAddress(0, 0, 5, 6));
                    cell.CellStyle = style;
                    row2.CreateCell(5).SetCellValue("Date: " + DateTime.Now.ToString("MM dd,yyyy hh:mm tt"));
                    excelSheet.AddMergedRegion(new CellRangeAddress(1, 1, 5, 6));

                    excelSheet.CreateFreezePane(0, 9, 0, 9);

                    //Search info
                    IRow s_row = excelSheet.CreateRow(8);
                    s_row.CreateCell(1).SetCellValue("Sales Person: " + ((sale_filted.sales_person == "all" || string.IsNullOrEmpty(sale_filted.sales_person)) ? "All" : db.P_Member.Where(m => m.MemberNumber == sale_filted.sales_person).FirstOrDefault().FullName));
                    s_row.CreateCell(3).SetCellValue("From: " + (sale_filted.fdate.HasValue ? sale_filted.fdate.Value.ToString("MMM dd, yyyy") : ""));
                    s_row.CreateCell(4).SetCellValue("To: " + (sale_filted.tdate.HasValue ? sale_filted.tdate.Value.ToString("MMM dd, yyyy") : ""));
                    s_row.CreateCell(6).SetCellValue("Data From: " + sale_filted.datafrom?.Replace("-", ""));
                    s_row.CreateCell(8).SetCellValue("Type: " + (sale_filted.Type == null ? "" : string.Join(", ", db.C_SalesLead_Status.Where(c => sale_filted.Type.Contains(c.Id.ToString())).Select(c => c.Name))));
                    s_row.CreateCell(10).SetCellValue("Status: " + db.C_SalesLead_Interaction_Status.Where(x => x.Id == sale_filted.Status).FirstOrDefault()?.Name);
                    s_row.CreateCell(12).SetCellValue("Search Text: " + sale_filted.SearchText);
                    //s_row.CreateCell(12).SetCellValue("Phone: " + sale_filted.phone);
                    //s_row.CreateCell(14).SetCellValue("Email: " + sale_filted.email);
                    //header table
                    IRow header = excelSheet.CreateRow(9);
                    string[] head_titles = { "#", "Salon Name", "Salon Email", "Salon Phone", "Salon Address", "Contact Name", "Contact Email", "Contact Phone", "Last Update", "Rate", "Status", "Sale Person", "Note" };
                    for (int i = 0; i < head_titles.Length; i++)
                    {
                        ICell c = header.CreateCell(i); c.SetCellValue(head_titles[i]); c.CellStyle = style1;
                    }

                    int row_num = 9;
                    //table data
                    int index = 1;
                    ICellStyle style2 = workbook.CreateCellStyle();
                    style2.WrapText = true;
                    foreach (var lead in sale_filted.query.ToList())
                    {
                        row_num++;

                        IRow row_next_1 = excelSheet.CreateRow(row_num);

                        row_next_1.CreateCell(0).SetCellValue(index);
                        //row_next_1.CreateCell(1).SetCellValue(lead.cus.CreateAt.Value.ToString("MM dd, yyyy hh:mm tt"));
                        row_next_1.CreateCell(1).SetCellValue(lead.cus.BusinessName);
                        row_next_1.CreateCell(2).SetCellValue(lead.cus.BusinessEmail);
                        row_next_1.CreateCell(3).SetCellValue(lead.cus.SalonPhone);
                        row_next_1.CreateCell(4).SetCellValue(lead.cus.BusinessAddress + ", " + lead.cus.BusinessCity + ", " + lead.cus.BusinessState + ", " + lead.cus.BusinessCountry + ", " + lead.cus.BusinessZipCode);
                        row_next_1.CreateCell(5).SetCellValue(lead.cus.ContactName);
                        row_next_1.CreateCell(6).SetCellValue(lead.cus.Email);
                        row_next_1.CreateCell(7).SetCellValue(lead.cus.CellPhone);
                        row_next_1.CreateCell(8).SetCellValue(lead.sl.UpdateAt.HasValue ? lead.sl.UpdateAt.Value.ToString("MMM dd,yyyy hh:mm tt") : "");
                        row_next_1.CreateCell(9).SetCellValue((lead.sl.PotentialRateScore ?? 0).ToString());
                        string status = "";
                        if (!string.IsNullOrEmpty(lead.sl.Interaction_Status))
                        {
                            status += lead.sl.Interaction_Status + Environment.NewLine;
                        }
                        if (!string.IsNullOrEmpty(lead.sl.CallOfNumber))
                        {
                            status += lead.sl.CallOfNumber + " called ";
                        }
                        row_next_1.CreateCell(10).SetCellValue(status);
                        row_next_1.GetCell(10).CellStyle = style2;
                        row_next_1.CreateCell(11).SetCellValue(db.P_Member.FirstOrDefault(x => x.MemberNumber == lead.sl.MemberNumber)?.FullName);
                        string DecodeLogTitle = "";
                        if (!string.IsNullOrEmpty(lead.LastLogTitle))
                        {
                            DecodeLogTitle = StripHTML(lead.LastLogTitle);
                        }
                        string DecodeLogDescription = "";
                        if (!string.IsNullOrEmpty(lead.LastLogDescription))
                        {
                            DecodeLogDescription = StripHTML(lead.LastLogDescription);
                        }
                        row_next_1.CreateCell(12).SetCellValue(DecodeLogTitle + Environment.NewLine + DecodeLogDescription);
                        row_next_1.GetCell(12).CellStyle = style2;
                        index++;
                    }
                    for (int i = 0; i < head_titles.Length; i++)
                    {
                        excelSheet.AutoSizeColumn(i);
                    }
                    workbook.Write(fs);
                }

                using (var fileStream = new FileStream(Server.MapPath(Path.Combine(webRootPath, fileName)), FileMode.Open))
                {
                    await fileStream.CopyToAsync(memoryStream);
                }
                memoryStream.Position = 0;
                string _fileName = DateTime.UtcNow.ToString("yyyyMMdd") + "_SaleLeadReport_" + (sale_filted.fdate.HasValue ? "From_" + sale_filted.fdate.Value.ToString("MMM_dd_yyyy") : "") + "_" + (sale_filted.tdate.HasValue ? "To_" + sale_filted.tdate.Value.ToString("MMM_dd_yyyy") : "") + ".xlsx";
                return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", _fileName);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public string StripHTML(string HTMLText, bool decode = true)
        {
            Regex reg = new Regex("<[^>]+>", RegexOptions.IgnoreCase);
            var stripped = reg.Replace(HTMLText, "");
            return decode ? HttpUtility.HtmlDecode(stripped) : stripped;
        }
        public string GetRemainingDays(string CustomerCode)
        {
            try
            {
                var customer = (from cus in db.C_Customer where cus.CustomerCode == CustomerCode join sl in db.C_SalesLead on cus.CustomerCode equals sl.CustomerCode select new { sl, cus }).FirstOrDefault();
                var store = db.Store_Services.Where(sub => sub.Active == 1 && sub.Type == "license" && sub.RenewDate != null && sub.CustomerCode == CustomerCode).FirstOrDefault();
                string result = "N/A";
                switch (customer.sl.SL_Status)
                {

                    //remaining trial account
                    case 2:
                        if (store == null)
                        {
                            result = "N/A";
                        }
                        else
                        {
                            result = CommonFunc.LicenseRemainingTime(store.RenewDate.Value);
                        }
                        break;
                    // remaining merchant
                    case 3:
                        if (store == null)
                        {
                            result = "N/A";
                        }
                        else
                        {
                            result = CommonFunc.LicenseRemainingTime(store.RenewDate.Value);
                        }
                        break;
                    // remaining slice
                    case 4:
                        if (customer.sl.L_IsVerify != true)
                        {
                            result = "waiting verify";
                        }
                        else
                        {
                            result = CommonFunc.LicenseRemainingTime(store.RenewDate.Value);
                        }
                        break;
                    // default
                    default:
                        break;
                }
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(CustomerCode);
            }

        }
        public Calendar_Event GetNextAppoiment(string SalesLeadId)
        {
            var ListAppoiment = db.Calendar_Event.Where(x => x.SalesLeadId == SalesLeadId && x.Type == "Event").ToList();
            if (ListAppoiment != null)
            {
                var nextAppoiment = ListAppoiment.Where(x => AppFunc.ParseTimeToUtc(x.StartEvent) > DateTime.UtcNow).OrderBy(x => AppFunc.ParseTimeToUtc(x.StartEvent)).FirstOrDefault();
                if (nextAppoiment != null)
                {
                    return nextAppoiment;
                }
            }
            return null;
        }
        //Update One Row Datatable Draw
        [HttpPost]
        public ActionResult CreateOrUpdateSalesLeadPopup(string SalesLeadId, string Page = "SalesLead")
        {
            var vm = new SalesLeadViewModel();
            // get sales lead for edit
            if (!string.IsNullOrEmpty(SalesLeadId))
            {
                var sl = db.C_SalesLead.Find(SalesLeadId);
                var customer = db.C_Customer.FirstOrDefault(x => x.CustomerCode == sl.CustomerCode);
                vm.Address = customer.BusinessAddress;
                vm.Id = sl.Id;
                vm.ContactName = customer.ContactName;
                vm.ContactPhone = customer.CellPhone;
                vm.SalonPhone = customer.BusinessPhone;
                vm.Address = customer.BusinessAddress ?? customer.SalonAddress1;
                vm.City = customer.BusinessCity;
                vm.State = customer.BusinessState;
                vm.SalonEmail = customer.BusinessEmail;
                vm.ZipCode = customer.Zipcode ?? customer.BusinessZipCode;
                vm.TimeZone = customer.SalonTimeZone;
                vm.SalonName = customer.BusinessName;
                vm.IsSendMail = sl.L_IsSendMail != null ? sl.L_IsSendMail.GetValueOrDefault() : false;
                vm.SalesPerson = sl.MemberNumber;
                vm.CallOfNumber = sl.CallOfNumber;
                vm.InteractionStatus = sl.Interaction_Status_Id;
                vm.PotentialRateScore = sl.PotentialRateScore ?? 1;
                vm.TeamNumber = sl.TeamNumber;
                // get member from Department sales
                if (!string.IsNullOrEmpty(sl.L_MoreInfo))
                    vm.MoreInfo = JsonConvert.DeserializeObject<MoreInfo>(sl.L_MoreInfo);
            }

            ViewBag.Page = Page;
            bool ShowTeam = false;
            // get member from Department sales
            var currentDeparmentsUser = db.P_Department.Where(d => d.Type == "SALES" && d.GroupMemberNumber.Contains(cMem.MemberNumber));
            if ((access.Any(k => k.Key.Equals("sales_lead_assigned")) == true && access["sales_lead_assigned"] == true))
            {
                var sale_dep = db.P_Department.Where(d => d.Type == "SALES").Select(d => d.Id).ToList().Select(d => d.ToString());
                if (access.Any(k => k.Key.Equals("sales_lead_view_all")) == true && access["sales_lead_view_all"] == true)
                {
                    ViewBag.ListMemberSales = db.P_Member.Where(m => m.Active == true && sale_dep.Any(n => m.DepartmentId.Contains(n))).ToList();
                    ShowTeam = true;
                }
                else if (access.Any(k => k.Key.Equals("sales_lead_view_team")) == true && access["sales_lead_view_team"] == true)
                {
                    List<string> listMemberNumber = new List<string>();
                    var ManagerDep = db.P_Department.Where(x => x.SupervisorNumber == cMem.MemberNumber && x.ParentDepartmentId != null && x.Type == "SALES").ToList();
                    if (ManagerDep.Count() > 0)
                    {
                        foreach (var dep in ManagerDep)
                        {
                            listMemberNumber.AddRange(dep.GroupMemberNumber.Split(','));
                        }
                        ShowTeam = true;
                    }
                    if (currentDeparmentsUser.Count() > 0)
                    {
                        foreach (var deparment in currentDeparmentsUser)
                        {
                            if (deparment.LeaderNumber == cMem.MemberNumber)
                            {
                                listMemberNumber.AddRange(deparment.GroupMemberNumber.Split(','));
                            }
                        }
                    }
                    listMemberNumber.Add(cMem.MemberNumber);
                    listMemberNumber = listMemberNumber.GroupBy(x => x).Select(x => x.Key).ToList();
                    ViewBag.ListMemberSales = (from m in db.P_Member join l in listMemberNumber on m.MemberNumber equals l select m).ToList();

                }
            }
            else
            {
                var sale_dep = db.P_Department.Where(d => d.Type == "SALES").Select(d => d.Id).ToList().Select(d => d.ToString());
                ViewBag.ListMemberSales = db.P_Member.Where(m => m.Active == true && sale_dep.Any(n => m.DepartmentId.Contains(n)) && m.MemberNumber.Contains(cMem.MemberNumber)).ToList();
            }
            ViewBag.ShowTeam = ShowTeam;
            ViewBag.State = db.Ad_USAState.Where(x => x.country == "USA").ToList();
            ViewBag.InteractionStatus = db.C_SalesLead_Interaction_Status.Where(x => x.Status == true).OrderBy(x => x.Order).Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name
            }).ToList();
            return PartialView("_SalesLead_CreateOrUpdate", vm);
        }
        /// <exclude />
        [HttpPost]
        public async Task<ActionResult> Update(SalesLeadViewModel model, string command)
        {

            SalesLeadService _salesLeadService = new SalesLeadService();
            var result = new Tuple<bool, string>(false, "");
            var Customer = new C_Customer();
            var Lead = new C_SalesLead();
            //insert status interaction
            var listTimeZone = new MerchantService().ListTimeZone();
            var listIMSTimeZone = new string[] { "Eastern", "Central", "Mountain", "Pacific", "VietNam" };
            // edit
            if (!string.IsNullOrEmpty(model.Id))
            {
                Lead = db.C_SalesLead.Find(model.Id);
                if (Lead == null)
                {
                    throw new Exception("Sales Lead Not Found!");
                }
                Customer = db.C_Customer.FirstOrDefault(x => x.CustomerCode == Lead.CustomerCode);
                if (Customer == null)
                {
                    throw new Exception("Customer Not Found!");
                }
                if (!string.IsNullOrEmpty(model.SalonEmail) && Customer.BusinessEmail != model.SalonEmail)
                {
                    if (db.C_Customer.Where(x => x.SalonEmail == model.SalonEmail).Count() > 0 || db.C_SalesLead.Where(x => x.L_Email == model.SalonEmail).Count() > 0)
                    {
                        return Json(new { status = false, message = "Email already exists !" });
                    }
                }
                // update Customer
                Customer.ContactName = Customer.OwnerName = model.ContactName;
                Customer.CellPhone = Customer.OwnerMobile = model.ContactPhone;
                Customer.Email = Customer.BusinessEmail = Customer.SalonEmail = Customer.MangoEmail = model.SalonEmail;
                Customer.BusinessName = model.SalonName;
                Customer.BusinessCountry = model.Country;
                Customer.BusinessAddressStreet = Customer.SalonAddress1 = Customer.BusinessAddress = model.Address?.Replace(",", " ");
                Customer.BusinessCity = Customer.SalonCity = Customer.City = model.City;
                Customer.BusinessState = Customer.SalonState = Customer.State = model.State;
                Customer.BusinessPhone = Customer.SalonPhone = model.SalonPhone;
                Customer.BusinessZipCode = Customer.SalonZipcode = Customer.Zipcode = model.ZipCode;
                if (!string.IsNullOrEmpty(model.TimeZone))
                {
                    if (Customer.SalonTimeZone != model.TimeZone)
                    {
                        Customer.SalonTimeZone = model.TimeZone;
                        if (Customer.SalonTimeZone == "Eastern" || Customer.SalonTimeZone == "Central" || Customer.SalonTimeZone == "Mountain" || Customer.SalonTimeZone == "Pacific" || Customer.SalonTimeZone == "VietNam")
                        {
                            Customer.SalonTimeZone_Number = Ext.EnumParse<TIMEZONE_NUMBER>(Customer.SalonTimeZone).Text();

                        }
                        else
                        {
                            Customer.SalonTimeZone_Number = listTimeZone.FirstOrDefault(t => t.Name == Customer.SalonTimeZone)?.TimeDT;
                        }
                    }

                }


                Customer.UpdateBy = cMem.FullName;
                db.Entry(Customer).State = EntityState.Modified;
                //update Lead
                Lead.L_City = model.City;
                Lead.L_State = model.State;
                Lead.L_Country = model.Country;
                Lead.L_Address = model.Address?.Replace(",", " ");
                Lead.L_ContactName = model.ContactName;
                Lead.L_ContactPhone = model.ContactPhone;
                Lead.L_Phone = model.SalonPhone;
                Lead.L_Email = model.SalonEmail;
                Lead.SalonTimeZone = Customer.SalonTimeZone;
                Lead.SalonTimeZone_Number = Customer.SalonTimeZone_Number;
                Lead.L_SalonName = model.SalonName;
                Lead.PotentialRateScore = model.PotentialRateScore;
                Lead.L_Zipcode = model.ZipCode;
                Lead.CustomerName = model.SalonName;
                Lead.L_Email = model.SalonEmail;
                if (model.InteractionStatus != null)
                {
                    var stt = db.C_SalesLead_Interaction_Status.Find(model.InteractionStatus);
                    Lead.Interaction_Status_Id = stt.Id;
                    Lead.Interaction_Status = stt.Name;

                }
                Lead.CallOfNumber = model.CallOfNumber;
                Lead.UpdateAt = DateTime.UtcNow;
                Lead.UpdateBy = cMem.FullName;
                if (!string.IsNullOrEmpty(model.SalesPerson))
                {
                    Lead.MemberNumber = model.SalesPerson;
                    Lead.MemberName = db.P_Member.Where(x => x.MemberNumber == model.SalesPerson).FirstOrDefault()?.FullName;
                    var notificationService = new NotificationService();
                    notificationService.SalesLeadAssignNotification(Lead.MemberNumber, Lead.Id, cMem.FullName, cMem.MemberNumber);
                }
                if (command == "create-trial")
                {

                    Lead.L_Version = "Trial";
                    Lead.L_CreateTrialBy = cMem.FullName;
                    Lead.L_CreateTrialAt = DateTime.UtcNow;
                }
                if (!string.IsNullOrEmpty(model.Note))
                {
                    Lead.LastNote = model.Note;
                    Lead.LastNoteAt = DateTime.UtcNow;
                }
                db.Entry(Lead).State = EntityState.Modified;
                db.SaveChanges();
                result = Tuple.Create(true, "Update success");
            }
            // create new 
            else
            {
                // set value for customer
                if (!string.IsNullOrEmpty(model.SalonEmail) && (db.C_Customer.Where(x => x.SalonEmail == model.SalonEmail).Count() > 0 || db.C_SalesLead.Where(x => x.L_Email == model.SalonEmail).Count() > 0))
                {
                    return Json(new { status = false, message = "Email already exists !" });
                }
                if (!string.IsNullOrEmpty(model.ContactPhone) && (db.C_Customer.Where(x => x.CellPhone == model.ContactPhone).Count() > 0 || db.C_SalesLead.Where(x => x.L_ContactPhone == model.ContactPhone).Count() > 0))
                {
                    return Json(new { status = false, message = "Contact Phone already exists !" });
                }
                if (!string.IsNullOrEmpty(model.SalonPhone) && (db.C_Customer.Where(x => x.SalonPhone == model.SalonPhone).Count() > 0 || db.C_SalesLead.Where(x => x.L_Phone == model.SalonPhone).Count() > 0))
                {
                    return Json(new { status = false, message = "Salon Phone already exists !" });
                }
              
                Customer.Id = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssfff"));
                Customer.CustomerCode = new MerchantService().MakeId();
                Customer.StoreCode = WebConfigurationManager.AppSettings["StorePrefix"] + Regex.Replace(Customer.CustomerCode, "[^.0-9]", "");
                Customer.BusinessEmail = Customer.SalonEmail = Customer.Email = model.SalonEmail;
                Customer.BusinessName = model.SalonName;
                Customer.OwnerName = Customer.ContactName = model.ContactName;
                Customer.OwnerMobile = Customer.CellPhone = model.ContactPhone;
                Customer.CreateAt = DateTime.UtcNow;
                Customer.CreateBy = cMem.FullName;
                Customer.CreateBy = cMem.FullName;
                Customer.UpdateBy = cMem.FullName;
                Customer.Password = db.SystemConfigurations.FirstOrDefault().MerchantPasswordDefault ?? string.Empty; // SecurityLibrary.Md5Encrypt(DateTime.UtcNow.ToString("O")).Substring(0, 6);
                Customer.MD5PassWord = SecurityLibrary.Md5Encrypt(Customer.Password);
                Customer.SalonEmail = Customer.BusinessEmail = model.SalonEmail;
                Customer.Country = Customer.BusinessCountry = model.Country;
                Customer.SalonAddress1 = Customer.BusinessAddressStreet = Customer.BusinessAddress = Customer.SalonAddress1 = model.Address?.Replace(",", " ");
                Customer.SalonCity = Customer.BusinessCity = Customer.City = model.City;
                Customer.SalonPhone = Customer.BusinessPhone = model.SalonPhone;
                Customer.SalonState = Customer.BusinessState = Customer.State = model.State;
                Customer.SalonZipcode = Customer.BusinessZipCode = Customer.Zipcode = model.ZipCode;
                //Customer.SalonTimeZone_Number = Ext.EnumParse<TIMEZONE_NUMBER_BY_ID>(model.TimeZone).Text();
                Customer.SalonTimeZone = model.TimeZone;
                Customer.Active = 1;
                if (cMem.SiteId > 1)
                {
                    Customer.PartnerCode = cMem.BelongToPartner;
                }
                Customer.SiteId = cMem.SiteId;
                if (!string.IsNullOrEmpty(model.TimeZone))
                {

                    if (listIMSTimeZone.Any(t => t.Contains(model.TimeZone)))
                    {
                        Customer.SalonTimeZone_Number = Ext.EnumParse<TIMEZONE_NUMBER>(model.TimeZone).Text();

                    }
                    else
                    {
                        Customer.SalonTimeZone_Number = listTimeZone.FirstOrDefault(t => t.Name == model.TimeZone)?.TimeDT;
                    }
                }
                Customer.More_Info = JsonConvert.SerializeObject(model.MoreInfo);

                //set partner code

                if (!string.IsNullOrEmpty(cMem.BelongToPartner))
                {
                    Customer.PartnerCode = cMem.BelongToPartner;
                }
                db.C_Customer.Add(Customer);
                await db.SaveChangesAsync();
                await EngineContext.Current.Resolve<IEnrichUniversalService>().InitialStoreDataAsync(Customer.StoreCode);

                // set value for sales lead
                Lead.Id = Guid.NewGuid().ToString();
                Lead.CustomerCode = Customer.CustomerCode;
                Lead.CustomerName = Lead.L_SalonName = model.SalonName;
                if (!string.IsNullOrEmpty(model.SalesPerson))
                {
                    Lead.MemberNumber = model.SalesPerson;
                    var salesPerson = db.P_Member.FirstOrDefault(x => x.MemberNumber == model.SalesPerson);
                    if (salesPerson != null)
                    {
                        await _salesLeadService.SendEmailAssigned(SalesPersonName: salesPerson.FullName, SalesLeadName: Lead.CustomerName, Phone: Lead.L_Phone, salesPerson.PersonalEmail);
                    }
                }
                Lead.L_Email = Customer.BusinessEmail;
                Lead.L_Country = model.Country;
                Lead.L_Address = model.Address?.Replace(",", "");
                Lead.L_State = model.State;
                Lead.L_City = model.City;
                Lead.L_ContactName = model.ContactName;
                Lead.L_ContactPhone = model.ContactPhone;
                Lead.L_Password = Customer.Password;
                Lead.L_Zipcode = model.ZipCode;
                if (model.InteractionStatus != null)
                {
                    var stt = db.C_SalesLead_Interaction_Status.Find(model.InteractionStatus);
                    Lead.Interaction_Status_Id = stt.Id;
                    Lead.Interaction_Status = stt.Name;
                }
                Lead.CallOfNumber = model.CallOfNumber;
                Lead.L_Type = LeadType.RegisterOnIMS.Text(); ;
                Lead.SL_Status = LeadStatus.Lead.Code<int>();
                Lead.SL_StatusName = LeadStatus.Lead.Text();
                Lead.PotentialRateScore = model.PotentialRateScore;
                Lead.CreateAt = DateTime.UtcNow;
                Lead.CreateBy = cMem.FullName;
                Lead.CreateByMemberNumber = cMem.MemberNumber;
                Lead.UpdateAt = DateTime.UtcNow;
                Lead.UpdateBy = cMem.FullName;
                Lead.L_MoreInfo = JsonConvert.SerializeObject(model.MoreInfo);
                Lead.SalonTimeZone = Customer.SalonTimeZone;
                Lead.SalonTimeZone_Number = Customer.SalonTimeZone_Number;
                if (command == "create-trial")
                {

                    Lead.L_Version = "Trial";
                    Lead.L_CreateTrialBy = cMem.FullName;
                    Lead.L_CreateTrialAt = DateTime.UtcNow;
                }
                if (!string.IsNullOrEmpty(model.Note))
                {
                    Lead.LastNote = model.Note;
                    Lead.LastNoteAt = DateTime.UtcNow;
                }
                db.C_SalesLead.Add(Lead);
                db.SaveChanges();
                if (!string.IsNullOrEmpty(Lead.MemberNumber))
                {
                    var notificationService = new NotificationService();
                    notificationService.SalesLeadAssignNotification(Lead.MemberNumber, Lead.Id, cMem.FullName, cMem.MemberNumber);
                }
                result = Tuple.Create(true, "Create New Sales Lead Success");
            }
            switch (command)
            {
                //action create trial account
                case "create-trial":
                    if (string.IsNullOrEmpty(Lead.L_Email))
                    {
                        return Json(new { status = false, message = "please enter email to create trial" });
                    }
                    await _salesLeadService.SendMailVerify(email: Lead.L_Email, name: Lead.L_ContactName, phone: Lead.L_Phone, link: WebConfigurationManager.AppSettings["HostRegisterMango"] + "/verify?key=" + SecurityLibrary.Encrypt(Lead.L_Email));
                    _salesLeadService.CreateLog(SalesLeadId: Lead.Id, SalesLeadName: Lead.L_SalonName, title: "The trial account is ready to be initiated.<br/> Email verify of the account was sent.", description: model.Note, MemberNumber: cMem.MemberNumber);
                    result = Tuple.Create(true, "Trial account is ready for activation, waiting for verification via email from customers");
                    break;
                // action verify
                case "verify":
                    if (string.IsNullOrEmpty(Lead.L_Email))
                    {
                        result = Tuple.Create(false, "please enter email to verify !");
                    }
                    if (Lead.SL_StatusName.Equals(LeadStatus.SliceAccount.Text()))
                        result = Tuple.Create(true, WebConfigurationManager.AppSettings["HostSliceMango"] + "/verify?key=" + SecurityLibrary.Encrypt(Lead.L_Email));
                    else
                        result = Tuple.Create(true, WebConfigurationManager.AppSettings["HostRegisterMango"] + "/verify?key=" + SecurityLibrary.Encrypt(Lead.L_Email));
                    break;
                // action resend email
                case "resend-email":
                    string domainName = HttpContext.Request.Url.GetLeftPart(UriPartial.Authority);

                    await _salesLeadService.SendMailVerify(email: Lead.L_Email, name: Lead.L_ContactName, phone: Lead.L_Phone, link: domainName + "/register/verify?key=" + SecurityLibrary.Encrypt(Lead.L_Email));
                    _salesLeadService.CreateLog(SalesLeadId: Lead.Id, SalesLeadName: Lead.L_SalonName, title: "Resend email verify success", description: model.Note, MemberNumber: cMem.MemberNumber);
                    result = Tuple.Create(true, "Resend email verify success");
                    break;
                // action saves
                default:
                    _salesLeadService.CreateLog(SalesLeadId: Lead.Id, SalesLeadName: Lead.L_SalonName, title: result.Item2, description: model.Note, MemberNumber: cMem.MemberNumber);
                    break;
            }

            if (result.Item1 == false)
            {
                return Json(new { status = false, message = result.Item2 });
            }
            //everything is ok
            return Json(new { status = true, message = result.Item2, SalesLeadId = Lead.Id, CustomerCode = Customer.CustomerCode });
        }
        [HttpPost]
        public async Task<ActionResult> ReAssigned(string SalesLeadId, string SalesPerson, string Type)
        {
            SalesLeadService _salesLeadService = new SalesLeadService();
            var sl = db.C_SalesLead.Find(SalesLeadId);

            if (sl == null)
            {
                return Json(new { status = false, message = "Sales Lead Not Found" });
            }
            if (string.IsNullOrEmpty(SalesPerson))
            {
                string nameFromSalesPerson = db.P_Member.FirstOrDefault(x => x.MemberNumber == sl.MemberNumber).FullName;
                sl.MemberNumber = null;
                sl.MemberName = null;
                sl.TeamNumber = null;
                sl.TeamName = null;
                db.SaveChanges();

                _salesLeadService.CreateLog(SalesLeadId: sl.Id, SalesLeadName: sl.CustomerName, title: "UnAssigned success", description: "UnAssigned " + nameFromSalesPerson + " success", MemberNumber: cMem.MemberNumber);
                return Json(new { status = true, message = "UnAssigned success" });
            }
            //Assigned
            else
            {
                if (Type == "Team")
                {
                    string NameFromTeam = "";
                    bool IsAssignedCommand = true;
                    if (sl.TeamNumber != null)
                    {
                        IsAssignedCommand = false;
                        NameFromTeam = db.P_Department.Where(x => x.Id == sl.TeamNumber).FirstOrDefault().Name;
                    }
                    long TeamId = long.Parse(SalesPerson);
                    string NameToTeam = db.P_Department.FirstOrDefault(x => x.Id == TeamId).Name;
                    sl.TeamNumber = long.Parse(SalesPerson);
                    sl.TeamName = NameToTeam;
                    sl.UpdateAt = DateTime.UtcNow;
                    sl.UpdateBy = cMem.MemberNumber;
                    db.SaveChanges();
                    if (!IsAssignedCommand)
                    {
                        _salesLeadService.CreateLog(SalesLeadId: sl.Id, SalesLeadName: sl.CustomerName, title: "ReAssigned Team success", description: "ReAssigned Team from : " + NameFromTeam + "  to " + NameToTeam, MemberNumber: cMem.MemberNumber);
                        return Json(new { status = true, message = "ReAssigned success" });
                    }
                    else
                    {
                        _salesLeadService.CreateLog(SalesLeadId: sl.Id, SalesLeadName: sl.CustomerName, title: "Assigned Team success", description: "Assigned Team to " + NameFromTeam + " success", MemberNumber: cMem.MemberNumber);
                        return Json(new { status = true, message = "Assigned success" });
                    }
                }
                else
                {
                    string nameFromSalesPerson = "";
                    bool IsAssignedCommand = true;
                    if (!string.IsNullOrEmpty(sl.MemberNumber))
                    {
                        IsAssignedCommand = false;
                        nameFromSalesPerson = db.P_Member.FirstOrDefault(x => x.MemberNumber == sl.MemberNumber).FullName;
                    }
                    string nameToSalesPerson = db.P_Member.FirstOrDefault(x => x.MemberNumber == SalesPerson).FullName;
                    sl.MemberNumber = SalesPerson;
                    sl.MemberName = nameToSalesPerson;
                    sl.UpdateAt = DateTime.UtcNow;
                    sl.UpdateBy = cMem.MemberNumber;
                    db.SaveChanges();

                    var salesPerson = db.P_Member.FirstOrDefault(x => x.MemberNumber == SalesPerson);
                    if (salesPerson != null)
                    {
                        var notificationService = new NotificationService();
                        notificationService.SalesLeadAssignNotification(sl.MemberNumber, sl.Id, cMem.FullName, cMem.MemberNumber);
                        await _salesLeadService.SendEmailAssigned(SalesPersonName: salesPerson.FullName, SalesLeadName: sl.CustomerName, Phone: sl.L_Phone, salesPerson.PersonalEmail);
                    }
                    if (!IsAssignedCommand)
                    {
                        _salesLeadService.CreateLog(SalesLeadId: sl.Id, SalesLeadName: sl.CustomerName, title: "ReAssigned success", description: "ReAssigned from " + nameFromSalesPerson + " to " + nameToSalesPerson, MemberNumber: cMem.MemberNumber);
                        return Json(new { status = true, message = "ReAssigned success" });
                    }
                    else
                    {
                        _salesLeadService.CreateLog(SalesLeadId: sl.Id, SalesLeadName: sl.CustomerName, title: "Assigned success", description: "Assigned to " + nameToSalesPerson + " success", MemberNumber: cMem.MemberNumber);
                        return Json(new { status = true, message = "Assigned success" });
                    }
                }
            }


        }
        #endregion

        #region New Register Tab
        [HttpPost]
        public ActionResult LoadState_NewRegister(string Page)
        {
            string TypeImportData = LeadType.ImportData.Text();
            int statusMerchant = LeadStatus.Merchant.Code<int>();
            int statusLead = LeadStatus.Lead.Code<int>();
            int statusTrial = LeadStatus.TrialAccount.Code<int>();
            int statusSlice = LeadStatus.SliceAccount.Code<int>();
            var ListState = new List<StateSearchSalesLeadModel>();
            bool showOtherState = false;
            var cusQuery = from sl in db.C_SalesLead where string.IsNullOrEmpty(sl.CustomerCode) select sl;
            if (Page == "NewRegister")
            {
                cusQuery = cusQuery.Where(x => x.L_Type != TypeImportData);
            }
            else
            {
                cusQuery = cusQuery.Where(x => x.L_Type == TypeImportData);
            }
            var queryState = from states in db.Ad_USAState select states;
            List<string> listState = new List<string>();
            List<string> listMemberNumber = new List<string>();
            List<long> listTeam = new List<long>();
            int totalRecord = 0;

            if ((access.Any(k => k.Key.Equals("sales_lead_view_all")) == true && access["sales_lead_view_all"] == true))
            {
                var query = from state in queryState
                            join cus in cusQuery.GroupBy(x => x.L_State) on state.abbreviation.Trim().ToLower() equals cus.Key.Trim().ToLower() into gj
                            from j in gj.DefaultIfEmpty()
                            select new StateSearchSalesLeadModel { Code = state.abbreviation, Name = state.name, Number = j.Key != null ? j.Count() : 0 };
                ListState.AddRange(query.ToList());
                totalRecord = query.Count();
                //ListState.AddRange(query.ToList());
                showOtherState = true;
            }
            else if (access.Any(k => k.Key.Equals("sales_lead_view_team")) == true && access["sales_lead_view_team"] == true)
            {
                var ManagerDep = db.P_Department.Where(x => x.SupervisorNumber == cMem.MemberNumber && x.ParentDepartmentId != null && x.Type == "SALES").ToList();
                if (ManagerDep.Count() > 0)
                {
                    foreach (var dep in ManagerDep)
                    {
                        listState.AddRange(dep.SaleStates.Split(','));
                        listMemberNumber.AddRange(dep.GroupMemberNumber.Split(','));
                    }
                }
                var currentDeparmentsUser = db.P_Department.Where(d => d.Type == "SALES" && d.GroupMemberNumber.Contains(cMem.MemberNumber));
                if (currentDeparmentsUser.Count() > 0)
                {
                    foreach (var deparment in currentDeparmentsUser)
                    {
                        listState.AddRange(deparment.SaleStates.Split(','));
                        listTeam.Add(deparment.Id);
                        if (deparment.LeaderNumber == cMem.MemberNumber)
                        {
                            listMemberNumber.AddRange(deparment.GroupMemberNumber.Split(','));
                        }
                    }
                }
                listMemberNumber.Add(cMem.MemberNumber);
                listMemberNumber = listMemberNumber.GroupBy(x => x).Select(x => x.Key).ToList();
                queryState = queryState.Where(x => listState.Any(y => y.Contains(x.abbreviation)));
                if (listState.Contains("Other"))
                {
                    showOtherState = true;
                }
                var query = from state in queryState
                            join cus in cusQuery.Where(x => listMemberNumber.Any(s => s.Contains(x.MemberNumber) || (string.IsNullOrEmpty(x.MemberNumber) && listTeam.Any(y => y == x.TeamNumber)) || string.IsNullOrEmpty(x.MemberNumber))).GroupBy(x => x.L_State) on state.abbreviation.Trim().ToLower() equals cus.Key.Trim().ToLower() into gj
                            from j in gj.DefaultIfEmpty()
                            select new StateSearchSalesLeadModel { Code = state.abbreviation, Name = state.name, Number = j.Key != null ? j.Count() : 0 };
                ListState.AddRange(query.ToList());
                totalRecord = query.Count();
            }
            return Json(new
            {
                showOtherState,
                data = ListState,
                recordsTotal = totalRecord
            });
        }

        [HttpPost]
        public ActionResult NewRegister_LoadList(IDataTablesRequest dataTableParam, string SearchText, int? Status, DateTime? FromDate, string Team, string SalesPerson, DateTime? ToDate, string State, string Page)
        {
            string TypeImportData = LeadType.ImportData.Text();
            var data = from sl in db.C_SalesLead
                       where string.IsNullOrEmpty(sl.CustomerCode)
                       join mb in db.P_Member on sl.MemberNumber equals mb.MemberNumber into gr
                       from mb in gr.DefaultIfEmpty()
                       join dp in db.P_Department on sl.TeamNumber equals dp.Id into grs
                       from dp in grs.DefaultIfEmpty()
                       select new { sl, mb, dp };
            if (Page == "NewRegister")
            {
                data = data.Where(x => x.sl.L_Type != TypeImportData);
            }
            else
            {
                data = data.Where(x => x.sl.L_Type == TypeImportData);
            }
            if (FromDate != null)
            {
                var From = FromDate.Value.Date + new TimeSpan(0, 0, 0);
                data = data.Where(x => x.sl.CreateAt >= From);
            }

            if (ToDate != null)
            {
                var To = ToDate.Value.Date + new TimeSpan(23, 59, 59);
                data = data.Where(x => x.sl.CreateAt <= To);
            }
            if (Status != null)
            {
                data = data.Where(x => x.sl.Interaction_Status_Id == Status);
            }
            if (!string.IsNullOrEmpty(SearchText))
            {
                data = data.Where(x => x.sl.L_ContactName.Contains(SearchText.Trim()) || x.sl.L_Phone.Contains(SearchText.Trim()) || (x.sl.L_Email.Contains(SearchText.Trim()) || x.sl.L_SalonName.Contains(SearchText.Trim())));
            }
            //filter data state
            if (!string.IsNullOrEmpty(State))
            {
                if (State.Contains("Other"))
                {
                    var ListAllState = db.Ad_USAState.Select(x => x.abbreviation).ToArray();
                    data = data.Where(x => State.Contains(x.sl.L_State) || !ListAllState.Contains(x.sl.L_State) || string.IsNullOrEmpty(x.sl.L_State));
                }
                else
                {
                    data = data.Where(x => State.Contains(x.sl.L_State));
                }
            }
            //filter data state
            if (!string.IsNullOrEmpty(Team))
            {
                List<string> listMemberNumber = new List<string>();
                List<string> listState = new List<string>();
                long TeamId = long.Parse(Team);
                var TeamFilter = db.P_Department.Find(long.Parse(Team));

                if (TeamFilter.SaleStates != null)
                    listState.AddRange(TeamFilter.SaleStates.Split(','));
                listMemberNumber.AddRange(TeamFilter.GroupMemberNumber.Split(','));
                if (listState.Count > 0)
                {
                    if (listState.Contains("Other"))
                    {
                        var ListAllState = db.Ad_USAState.Select(x => x.abbreviation).ToArray();
                        data = data.Where(x => listMemberNumber.Any(y => y.Contains(x.sl.MemberNumber)) || (string.IsNullOrEmpty(x.sl.MemberNumber) && x.sl.TeamNumber == TeamId) || ((!ListAllState.Contains(x.sl.L_State) || string.IsNullOrEmpty(x.sl.L_State) || listState.Any(s => s.Contains(x.sl.L_State))) && string.IsNullOrEmpty(x.sl.MemberNumber) && x.sl.TeamNumber == null));
                    }
                    else
                    {
                        data = data.Where(x => listMemberNumber.Any(y => y.Contains(x.sl.MemberNumber)) || (string.IsNullOrEmpty(x.sl.MemberNumber) && x.sl.TeamNumber == TeamId) || (listState.Any(s => s.Contains(x.sl.L_State)) && string.IsNullOrEmpty(x.sl.MemberNumber) && x.sl.TeamNumber == null));
                    }
                }
                else
                {
                    data = data.Where(x => listMemberNumber.Any(y => y.Contains(x.sl.MemberNumber)) || (string.IsNullOrEmpty(x.sl.MemberNumber) && x.sl.TeamNumber == TeamId));
                }
            }
            if (!string.IsNullOrEmpty(SalesPerson))
            {
                if (SalesPerson == "Unassigned")
                {
                    data = data.Where(x => string.IsNullOrEmpty(x.sl.MemberNumber));
                }
                else
                {
                    data = data.Where(x => x.sl.MemberNumber == SalesPerson);
                }
            }
            var currentDeparmentsUser = db.P_Department.Where(d => d.Type == "SALES" && d.GroupMemberNumber.Contains(cMem.MemberNumber));
            if (!AppFunc.CanAccess("sales_lead_view_all"))
            {
                if (AppFunc.CanAccess("sales_lead_view_team"))
                {
                    List<string> listMemberNumber = new List<string>();
                    List<string> listState = new List<string>();
                    List<long> listTeam = new List<long>();
                    var ManagerDep = db.P_Department.Where(x => x.SupervisorNumber == cMem.MemberNumber && x.ParentDepartmentId != null && x.Type == "SALES").ToList();
                    if (ManagerDep.Count() > 0)
                    {
                        foreach (var dep in ManagerDep)
                        {
                            listState.AddRange(dep.SaleStates.Split(','));
                            listMemberNumber.AddRange(dep.GroupMemberNumber.Split(','));
                        }
                    }
                    if (currentDeparmentsUser.Count() > 0)
                    {
                        foreach (var deparment in currentDeparmentsUser)
                        {
                            listState.AddRange(deparment.SaleStates.Split(','));
                            listTeam.Add(deparment.Id);
                            if (deparment.LeaderNumber == cMem.MemberNumber)
                            {
                                listMemberNumber.AddRange(deparment.GroupMemberNumber.Split(','));
                            }
                        }
                    }
                    listMemberNumber.Add(cMem.MemberNumber);
                    if (listState.Count > 0)
                    {
                        if (listState.Contains("Other"))
                        {
                            var ListAllState = db.Ad_USAState.Select(x => x.abbreviation).ToArray();
                            data = data.Where(x => listMemberNumber.Any(y => y.Contains(x.sl.MemberNumber)) || (string.IsNullOrEmpty(x.sl.MemberNumber) && listTeam.Any(y => y == (x.sl.TeamNumber))) || ((!ListAllState.Contains(x.sl.L_State) || string.IsNullOrEmpty(x.sl.L_State) || listState.Any(s => s.Contains(x.sl.L_State))) && string.IsNullOrEmpty(x.sl.MemberNumber) && x.sl.TeamNumber == null));
                        }
                        else
                        {
                            data = data.Where(x => listMemberNumber.Any(y => y.Contains(x.sl.MemberNumber)) || (string.IsNullOrEmpty(x.sl.MemberNumber) && listTeam.Any(y => y == (x.sl.TeamNumber))) || (listState.Any(s => s.Contains(x.sl.L_State)) && string.IsNullOrEmpty(x.sl.MemberNumber) && x.sl.TeamNumber == null));
                        }

                    }
                    else
                    {
                        data = data.Where(x => listMemberNumber.Any(y => y.Contains(x.sl.MemberNumber)) || (string.IsNullOrEmpty(x.sl.MemberNumber) && listTeam.Any(y => y == (x.sl.TeamNumber))));
                    }

                }
            }
            var totalRecord = data.Count();
            var colSearch = dataTableParam.Columns.Where(c => c.Sort != null).FirstOrDefault();
            switch (colSearch.Name)
            {
                case "LastUpdate":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        data = data.OrderBy(s => s.sl.UpdateAt);
                    }
                    else
                    {
                        data = data.OrderByDescending(s => s.sl.UpdateAt);
                    }

                    break;
                case "Note":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        data = data.OrderBy(s => s.sl.LastNoteAt);
                    }
                    else
                    {
                        data = data.OrderByDescending(s => s.sl.LastNoteAt);
                    }
                    break;
                case "L_Version":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        data = data.OrderBy(s => s.sl.L_Version);
                    }
                    else
                    {
                        data = data.OrderByDescending(s => s.sl.L_Version);
                    }
                    break;
                case "L_State":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        data = data.OrderBy(s => s.sl.L_State);
                    }
                    else
                    {
                        data = data.OrderByDescending(s => s.sl.L_State);
                    }
                    break;
                case "InteractionStatus":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        data = data.OrderBy(s => s.sl.Interaction_Status);
                    }
                    else
                    {
                        data = data.OrderByDescending(s => s.sl.Interaction_Status);
                    }
                    break;
                case "AssignedSalesPerson":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        data = data.OrderBy(s => s.sl.MemberNumber).ThenBy(s => s.sl.TeamNumber);
                    }
                    else
                    {
                        data = data.OrderByDescending(s => s.sl.MemberNumber).ThenByDescending(s => s.sl.TeamNumber);
                    }
                    break;
                default:
                    data = data.OrderByDescending(s => s.sl.CreateAt);
                    break;
            }

            Session["RIData_filted"] = new RI_filted
            {
                DataFrom = Page,
                fdate = FromDate,
                tdate = ToDate,
                query = data.Select(x => new RI_filted.Query_RI_data { LastLogTitle = "Note", LastLogDescription = x.sl.LastNote, sl = x.sl })
            };
            data = data.Skip(dataTableParam.Start).Take(dataTableParam.Length);//.SortAndPage(dataTableParam);
            var modeldata = data.ToList();
            try
            {
                var ViewData = modeldata.Select(x => new
                {
                    Id = x.sl.Id,
                    L_ContactName = x.sl.L_ContactName,
                    L_SalonName = x.sl.L_SalonName ?? string.Empty,
                    L_ContactPhone = x.sl.L_ContactPhone,
                    L_Phone = x.sl.L_Phone,
                    L_Email = x.sl.L_Email,
                    L_State = x.sl.L_State,
                    L_Product = x.sl.L_Product ?? "",
                    VerifyUrl = WebConfigurationManager.AppSettings["HostRegisterMango"] + "/verify?key=" + SecurityLibrary.Encrypt(x.sl.L_Email),
                    TimeSendMail = x.sl.L_CreateTrialAt != null ? CommonFunc.DateTimeRemain(x.sl.L_CreateTrialAt.GetValueOrDefault()) : null,
                    StatusVerify = x.sl.L_IsSendMail != null ? x.sl.L_IsSendMail.GetValueOrDefault() : false,
                    CreateAt = x.sl.CreateAt != null ? string.Format("{0:r}", x.sl.CreateAt.GetValueOrDefault()) : "",
                    UpdateAt = x.sl.UpdateAt != null ? string.Format("{0:r}", x.sl.UpdateAt.GetValueOrDefault()) : "",
                    L_Version = x.sl.L_Version,
                    AssignedSalesPerson = x.mb?.FullName ?? string.Empty,
                    AssignedSalesPersonNotMatch = string.IsNullOrEmpty(x.mb?.FullName) ? (x.sl.MemberName ?? string.Empty) : string.Empty,
                    AssignedTeam = x.dp?.Name ?? "",
                    Color = x.mb?.ProfileDefinedColor ?? "#ffffff",
                    InteractionStatus = x.sl.Interaction_Status ?? "",
                    CallOfNumber = x.sl.CallOfNumber ?? "",
                    Note = x.sl.LastNote ?? "",
                    License_Name = x.sl.L_License_Name ?? "",
                    NoteTime = x.sl.LastNoteAt != null ? string.Format("{0:r}", x.sl.LastNoteAt.GetValueOrDefault()) : "",
                    RelativeTime = string.Format("{0:r}", x.sl.CreateAt)
                });
                return Json(new
                {
                    recordsTotal = totalRecord,
                    recordsFiltered = totalRecord,
                    draw = dataTableParam.Draw,
                    data = ViewData
                });
            }
            catch (Exception e)
            {
                //       System.Diagnostics.Debug.WriteLine(e.Message);
                return Json(new
                {
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    draw = dataTableParam.Draw,
                    data = new List<C_SalesLead>()
                });
            }
        }

        public async Task<FileStreamResult> ExportExcelRI()
        {
            try
            {
                int statusMerchant = LeadStatus.Merchant.Code<int>();
                var webinfo = db.SystemConfigurations.FirstOrDefault();
                string[] address = webinfo?.CompanyAddress?.Split(new char[] { '|' });
                var sale_filted = Session["RIData_filted"] as RI_filted;
                if (sale_filted == null)
                {
                    throw null;
                }
                //view all sales lead if permission is view all
                string webRootPath = "/upload/other/";
                string fileName = @"TempData.xlsx";
                var memoryStream = new MemoryStream();
                // --- Below code would create excel file with dummy data----  
                using (var fs = new FileStream(Server.MapPath(Path.Combine(webRootPath, fileName)), FileMode.Create, FileAccess.Write))
                {

                    IWorkbook workbook = new XSSFWorkbook();
                    //name style
                    IFont font = workbook.CreateFont();
                    font.IsBold = true;
                    font.FontHeightInPoints = 12;
                    ICellStyle style = workbook.CreateCellStyle();
                    style.SetFont(font);
                    //header style
                    IFont font1 = workbook.CreateFont();
                    font1.IsBold = true;
                    font1.Underline = FontUnderlineType.Double;
                    font1.FontHeightInPoints = 13;
                    ICellStyle style1 = workbook.CreateCellStyle();
                    style1.SetFont(font1);
                    IDataFormat dataFormatCustom = workbook.CreateDataFormat();
                    string title = "";
                    if (sale_filted.DataFrom == "NewRegister")
                    {
                        title = "New Register Report";
                    }
                    else
                    {
                        title = "Import Data Report";
                    }
                    ISheet excelSheet = workbook.CreateSheet(title);
                    //set column width
                    excelSheet.SetColumnWidth(1, 15 * 256);
                    excelSheet.SetColumnWidth(2, 15 * 256);
                    excelSheet.SetColumnWidth(3, 20 * 256);
                    excelSheet.SetColumnWidth(4, 10 * 256);
                    excelSheet.SetColumnWidth(5, 20 * 256);
                    excelSheet.SetColumnWidth(6, 15 * 256);
                    excelSheet.SetColumnWidth(7, 20 * 256);
                    excelSheet.SetColumnWidth(8, 20 * 256);
                    //reprot info
                    IRow row1 = excelSheet.CreateRow(0);
                    row1.CreateCell(0).SetCellValue(webinfo?.CompanyName);
                    excelSheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 1));
                    IRow row2 = excelSheet.CreateRow(1);
                    row2.CreateCell(0).SetCellValue(address.Length > 0 ? address[0] : "---");
                    excelSheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 1));
                    IRow row3 = excelSheet.CreateRow(2);
                    row3.CreateCell(0).SetCellValue(address.Length > 0 ? address[1] + "," + address[2] + address[3] : "---,--- #####");
                    excelSheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 1));
                    IRow row4 = excelSheet.CreateRow(3);
                    row4.CreateCell(0).SetCellValue("www.enrichcous.com");
                    excelSheet.AddMergedRegion(new CellRangeAddress(3, 3, 0, 1));
                    ICell cell = row1.CreateCell(5);
                    cell.SetCellValue(new XSSFRichTextString(title));
                    excelSheet.AddMergedRegion(new CellRangeAddress(0, 0, 5, 6));
                    cell.CellStyle = style;
                    row2.CreateCell(5).SetCellValue("Date: " + DateTime.Now.ToString("MM dd,yyyy hh:mm tt"));
                    excelSheet.AddMergedRegion(new CellRangeAddress(1, 1, 5, 6));

                    excelSheet.CreateFreezePane(0, 9, 0, 9);

                    ////Search info
                    //IRow s_row = excelSheet.CreateRow(8);
                    //s_row.CreateCell(1).SetCellValue("Sales person: " + ((sale_filted.sales_person == "all" || string.IsNullOrEmpty(sale_filted.sales_person)) ? "All" : db.P_Member.Where(m => m.MemberNumber == sale_filted.sales_person).FirstOrDefault().FullName));
                    //s_row.CreateCell(3).SetCellValue("From: " + (sale_filted.fdate.HasValue ? sale_filted.fdate.Value.ToString("MMM dd, yyyy") : ""));
                    //s_row.CreateCell(4).SetCellValue("To: " + (sale_filted.tdate.HasValue ? sale_filted.tdate.Value.ToString("MMM dd, yyyy") : ""));
                    //s_row.CreateCell(6).SetCellValue("Data From: " + sale_filted.datafrom.Replace("-", ""));
                    //s_row.CreateCell(8).SetCellValue("Status: " + (sale_filted.status == null ? "" : db.C_SalesLead_Status.Find(sale_filted.status)?.Name));

                    //s_row.CreateCell(10).SetCellValue("Name: " + sale_filted.name);
                    //s_row.CreateCell(12).SetCellValue("Phone: " + sale_filted.phone);
                    //s_row.CreateCell(14).SetCellValue("Email: " + sale_filted.email);
                    //header table
                    IRow header = excelSheet.CreateRow(8);
                    string[] head_titles = { "#", "Salon Name", "Salon Email", "Salon Address", "Contact Name", "Contact Email", "Phone", "Last Update", "Rate", "Status", "Sale Person", "Note" };
                    for (int i = 0; i < head_titles.Length; i++)
                    {
                        ICell c = header.CreateCell(i); c.SetCellValue(head_titles[i]); c.CellStyle = style1;
                    }

                    int row_num = 9;
                    //table data
                    int index = 1;
                    ICellStyle style2 = workbook.CreateCellStyle();
                    style2.WrapText = true;
                    foreach (var lead in sale_filted.query.ToList())
                    {
                        row_num++;

                        IRow row_next_1 = excelSheet.CreateRow(row_num);
                        row_next_1.CreateCell(0).SetCellValue(index);
                        //row_next_1.CreateCell(1).SetCellValue(lead.cus.CreateAt.Value.ToString("MM dd, yyyy hh:mm tt"));
                        row_next_1.CreateCell(1).SetCellValue(lead.sl.L_SalonName);
                        row_next_1.CreateCell(2).SetCellValue(lead.sl.L_Email);
                        row_next_1.CreateCell(3).SetCellValue(lead.sl.L_Address + ", " + lead.sl.L_City + ", " + lead.sl.L_State + ", " + lead.sl.L_Country + ", " + lead.sl.L_Zipcode);
                        row_next_1.CreateCell(4).SetCellValue(lead.sl.L_ContactName);
                        row_next_1.CreateCell(5).SetCellValue(lead.sl.L_Email);
                        row_next_1.CreateCell(6).SetCellValue(lead.sl.L_Phone);
                        row_next_1.CreateCell(7).SetCellValue(lead.sl.UpdateAt.HasValue ? lead.sl.UpdateAt.Value.ToString("MMM dd,yyyy hh:mm tt") : "");
                        row_next_1.CreateCell(8).SetCellValue((lead.sl.PotentialRateScore ?? 0).ToString());
                        string status = "";
                        if (!string.IsNullOrEmpty(lead.sl.Interaction_Status))
                        {
                            status += lead.sl.Interaction_Status + Environment.NewLine;
                        }
                        if (!string.IsNullOrEmpty(lead.sl.CallOfNumber))
                        {
                            status += lead.sl.CallOfNumber + " called ";
                        }
                        row_next_1.CreateCell(9).SetCellValue(status);
                        row_next_1.GetCell(9).CellStyle = style2;
                        row_next_1.CreateCell(10).SetCellValue(db.P_Member.FirstOrDefault(x => x.MemberNumber == lead.sl.MemberNumber)?.FullName);
                        string DecodeLogTitle = "";
                        if (!string.IsNullOrEmpty(lead.LastLogTitle))
                        {
                            DecodeLogTitle = StripHTML(lead.LastLogTitle);
                        }
                        string DecodeLogDescription = "";
                        if (!string.IsNullOrEmpty(lead.LastLogDescription))
                        {
                            DecodeLogDescription = StripHTML(lead.LastLogDescription);
                        }
                        row_next_1.CreateCell(11).SetCellValue(DecodeLogTitle + Environment.NewLine + DecodeLogDescription);
                        row_next_1.GetCell(11).CellStyle = style2;
                        index++;
                    }
                    for (int i = 0; i < head_titles.Length; i++)
                    {
                        excelSheet.AutoSizeColumn(i);
                    }
                    workbook.Write(fs);
                }

                using (var fileStream = new FileStream(Server.MapPath(Path.Combine(webRootPath, fileName)), FileMode.Open))
                {
                    await fileStream.CopyToAsync(memoryStream);
                }
                memoryStream.Position = 0;
                string _fileName = DateTime.UtcNow.ToString("yyyyMMdd") + "_RIReport_" + (sale_filted.fdate.HasValue ? "From_" + sale_filted.fdate.Value.ToString("MMM_dd_yyyy") : "") + "_" + (sale_filted.tdate.HasValue ? "To_" + sale_filted.tdate.Value.ToString("MMM_dd_yyyy") : "") + ".xlsx";
                return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", _fileName);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public ActionResult SalesLead_UpdateRow_RI(string salesLeadId)
        {
            var data = from sl in db.C_SalesLead
                       where sl.Id == salesLeadId
                       join mb in db.P_Member on sl.MemberNumber equals mb.MemberNumber into gr
                       from mb in gr.DefaultIfEmpty()
                       join dp in db.P_Department on sl.TeamNumber equals dp.Id into grs
                       from dp in grs.DefaultIfEmpty()
                       select new { sl, mb, dp };
            var x = data.FirstOrDefault();
            var ViewData = new
            {
                Id = x.sl.Id,
                L_ContactName = x.sl.L_ContactName,
                L_SalonName = x.sl.L_SalonName ?? string.Empty,
                L_ContactPhone = x.sl.L_ContactPhone,
                L_Phone = x.sl.L_Phone,
                L_Email = x.sl.L_Email,
                L_State = x.sl.L_State,
                L_Product = x.sl.L_Product ?? "N/A",
                VerifyUrl = WebConfigurationManager.AppSettings["HostRegisterMango"] + "/verify?key=" + SecurityLibrary.Encrypt(x.sl.L_Email),
                TimeSendMail = x.sl.L_CreateTrialAt != null ? CommonFunc.DateTimeRemain(x.sl.L_CreateTrialAt.GetValueOrDefault()) : null,
                StatusVerify = x.sl.L_IsSendMail != null ? x.sl.L_IsSendMail.GetValueOrDefault() : false,
                CreateAt = x.sl.CreateAt != null ? string.Format("{0:r}", x.sl.CreateAt.GetValueOrDefault()) : "",
                UpdateAt = x.sl.UpdateAt != null ? string.Format("{0:r}", x.sl.UpdateAt.GetValueOrDefault()) : "",
                L_Version = x.sl.L_Version,
                AssignedSalesPerson = x.mb?.FullName ?? "",
                AssignedTeam = x.dp?.Name ?? "",
                Color = x.mb?.ProfileDefinedColor ?? "#ffffff",
                InteractionStatus = x.sl.Interaction_Status ?? "",
                CallOfNumber = x.sl.CallOfNumber ?? "",
                Note = x.sl.LastNote ?? "",
                License_Name = x.sl.L_License_Name ?? "",
                NoteTime = x.sl.LastNoteAt != null ? string.Format("{0:r}", x.sl.LastNoteAt.GetValueOrDefault()) : "",
                RelativeTime = string.Format("{0:r}", x.sl.CreateAt)
            };
            return Json(new
            {
                data = ViewData
            });
        }

        [HttpPost]
        public ActionResult NewRegister_ViewDetail(string id)
        {
            var sl = db.C_SalesLead.Where(x => x.Id == id).FirstOrDefault();
            var vm = new SalesLeadViewModel();
            bool ShowTeam = false;
            if (sl != null)
            {
                vm.Id = sl.Id;
                vm.ContactName = sl.L_ContactName;
                vm.ContactPhone = vm.SalonPhone = sl.L_ContactPhone;
                vm.Address = sl.L_Address;
                vm.City = sl.L_City;
                vm.State = sl.L_State;
                vm.SalonEmail = sl.L_Email;
                vm.Password = sl.L_Password;
                vm.SalonName = sl.L_SalonName;
                vm.IsSendMail = sl.L_IsSendMail != null ? sl.L_IsSendMail.GetValueOrDefault() : false;
                vm.SalesPerson = sl.MemberNumber;
                vm.ZipCode = sl.L_Zipcode;
                vm.PotentialRateScore = sl.PotentialRateScore ?? 0;
                vm.TimeZone = sl.SalonTimeZone;
                vm.TimeZoneNumber = sl.SalonTimeZone_Number;
                vm.CallOfNumber = sl.CallOfNumber;
                vm.InteractionStatus = sl.Interaction_Status_Id;
                vm.TeamNumber = sl.TeamNumber;
                var currentDeparmentsUser = db.P_Department.Where(d => d.Type == "SALES" && d.GroupMemberNumber.Contains(cMem.MemberNumber));
                // get member from Department sales
                if ((access.Any(k => k.Key.Equals("sales_lead_assigned")) == true && access["sales_lead_assigned"] == true))
                {
                    var sale_dep = db.P_Department.Where(d => d.Type == "SALES").Select(d => d.Id).ToList().Select(d => d.ToString());
                    if (access.Any(k => k.Key.Equals("sales_lead_view_all")) == true && access["sales_lead_view_all"] == true)
                    {
                        ViewBag.ListMemberSales = db.P_Member.Where(m => m.Active == true && sale_dep.Any(n => m.DepartmentId.Contains(n))).ToList();
                        ShowTeam = true;
                    }
                    else if (access.Any(k => k.Key.Equals("sales_lead_view_team")) == true && access["sales_lead_view_team"] == true)
                    {
                        List<string> listMemberNumber = new List<string>();
                        var ManagerDep = db.P_Department.Where(x => x.SupervisorNumber == cMem.MemberNumber && x.ParentDepartmentId != null && x.Type == "SALES").ToList();
                        if (ManagerDep.Count() > 0)
                        {
                            foreach (var dep in ManagerDep)
                            {
                                listMemberNumber.AddRange(dep.GroupMemberNumber.Split(','));
                            }
                            ShowTeam = true;
                        }
                        if (currentDeparmentsUser.Count() > 0)
                        {
                            foreach (var deparment in currentDeparmentsUser)
                            {
                                if (deparment.LeaderNumber == cMem.MemberNumber)
                                {
                                    listMemberNumber.AddRange(deparment.GroupMemberNumber.Split(','));
                                }
                            }
                        }
                        listMemberNumber.Add(cMem.MemberNumber);
                        listMemberNumber = listMemberNumber.GroupBy(x => x).Select(x => x.Key).ToList();
                        ViewBag.ListMemberSales = (from m in db.P_Member join l in listMemberNumber on m.MemberNumber equals l select m).ToList();
                    }
                }
                else
                {
                    var sale_dep = db.P_Department.Where(d => d.Type == "SALES").Select(d => d.Id).ToList().Select(d => d.ToString());
                    ViewBag.ListMemberSales = db.P_Member.Where(m => m.Active == true && sale_dep.Any(n => m.DepartmentId.Contains(n)) && m.MemberNumber.Contains(cMem.MemberNumber)).ToList();
                }
                ViewBag.ShowTeam = ShowTeam;
                ViewBag.Page = "NewRegister";
                if (!string.IsNullOrEmpty(sl.L_MoreInfo))
                    vm.MoreInfo = JsonConvert.DeserializeObject<MoreInfo>(sl.L_MoreInfo);
                ViewBag.State = db.Ad_USAState.Where(x => x.country == "USA").ToList();
                ViewBag.InteractionStatus = db.C_SalesLead_Interaction_Status.Where(x => x.Status == true).OrderBy(x => x.Order).Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                }).ToList();
                return PartialView("_SalesLead_CreateOrUpdate", vm);
            }
            else
            {
                ViewBag.Page = "NewRegister";
                ViewBag.State = db.Ad_USAState.Where(x => x.country == "USA").ToList();
                ViewBag.InteractionStatus = db.C_SalesLead_Interaction_Status.Where(x => x.Status == true).OrderBy(x => x.Order).Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                }).ToList();
                return PartialView("_SalesLead_CreateOrUpdate", vm);
            }
        }
        [HttpPost]
        public async Task<ActionResult> NewRegister_Update(SalesLeadViewModel model, string command)
        {
            SalesLeadService _salesLeadService = new SalesLeadService();
            var sl = db.C_SalesLead.Where(x => x.Id == model.Id).FirstOrDefault();
            if (sl == null)
            {
                throw new ArgumentNullException("Sales Lead Not Found");
            }
            if (!string.IsNullOrEmpty(model.SalonEmail) && sl.L_Email != model.SalonEmail)
            {
                if (db.C_Customer.Where(x => x.SalonEmail == model.SalonEmail).Count() > 0 || db.C_SalesLead.Where(x => x.L_Email == model.SalonEmail).Count() > 0)
                {
                    return Json(new { status = false, message = "Email already exists!" });
                }
            }
            var result = new Tuple<bool, string>(false, "");
            sl.L_Address = model.Address;
            if (!string.IsNullOrEmpty(model.SalesPerson) && model.SalesPerson != sl.MemberNumber)
            {
                sl.MemberNumber = model.SalesPerson;
                var salesPerson = db.P_Member.FirstOrDefault(x => x.MemberNumber == model.SalesPerson);
                if (salesPerson != null)
                {
                    await _salesLeadService.SendEmailAssigned(SalesPersonName: salesPerson.FullName, SalesLeadName: model.SalonName, Phone: model.SalonPhone, salesPerson.PersonalEmail);
                }
            }
            sl.L_City = model.City;
            sl.L_State = model.State;
            sl.L_Country = "United States";
            sl.L_Address = model.Address?.Replace(",", "");
            sl.L_ContactName = model.ContactName;
            sl.L_ContactPhone = model.ContactPhone;
            sl.L_Phone = model.SalonPhone;
            sl.L_Email = model.SalonEmail;
            sl.L_SalonName = model.SalonName;
            sl.PotentialRateScore = model.PotentialRateScore;
            sl.L_Zipcode = model.ZipCode;
            sl.SalonTimeZone = model.TimeZone;
            if (model.InteractionStatus != null)
            {
                var stt = db.C_SalesLead_Interaction_Status.Find(model.InteractionStatus);
                sl.Interaction_Status_Id = stt.Id;
                sl.Interaction_Status = stt.Name;

            }
            sl.CallOfNumber = model.CallOfNumber;

            if (sl.SalonTimeZone != model.TimeZone)
            {
                var listTimeZone = new MerchantService().ListTimeZone();
                var listIMSTimeZone = new string[] { "Eastern", "Central", "Mountain", "Pacific", "VietNam" };
                if (listIMSTimeZone.Any(t => t.Contains(model.TimeZone)))
                {
                    sl.SalonTimeZone_Number = Ext.EnumParse<TIMEZONE_NUMBER>(model.TimeZone).Text();

                }
                else
                {
                    sl.SalonTimeZone_Number = listTimeZone.FirstOrDefault(t => t.Name == model.TimeZone)?.TimeDT;
                }

            }

            //// get member from Department sales
            //var currentDeparmentsUser = db.P_Department.Where(d => d.Type == "SALES" && d.GroupMemberNumber.Contains(cMem.MemberNumber));
            //if ((access.Any(k => k.Key.Equals("sales_lead_assigned")) == true && access["sales_lead_assigned"] == true))
            //{
            //    var sale_dep = db.P_Department.Where(d => d.Type == "SALES").Select(d => d.Id).ToList().Select(d => d.ToString());
            //    if (access.Any(k => k.Key.Equals("sales_lead_view_all")) == true && access["sales_lead_view_all"] == true)
            //    {
            //        ViewBag.ListMemberSales = db.P_Member.Where(m => m.Active == true && sale_dep.Any(n => m.DepartmentId.Contains(n))).ToList();
            //    }
            //    else if (access.Any(k => k.Key.Equals("sales_lead_view_team")) == true && access["sales_lead_view_team"] == true)
            //    {
            //        List<string> listMemberNumber = new List<string>();
            //        if (currentDeparmentsUser.Count() > 0)
            //        {
            //            foreach (var deparment in currentDeparmentsUser)
            //            {
            //                if (deparment.LeaderNumber == cMem.MemberNumber)
            //                {
            //                    listMemberNumber.AddRange(deparment.GroupMemberNumber.Split(','));
            //                }
            //            }
            //        }
            //        listMemberNumber.Add(cMem.MemberNumber);
            //        listMemberNumber = listMemberNumber.GroupBy(x => x).Select(x => x.Key).ToList();
            //        ViewBag.ListMemberSales = (from m in db.P_Member join l in listMemberNumber on m.MemberNumber equals l select m).ToList();
            //    }
            //}
            //else
            //{
            //    var sale_dep = db.P_Department.Where(d => d.Type == "SALES").Select(d => d.Id).ToList().Select(d => d.ToString());
            //    ViewBag.ListMemberSales = db.P_Member.Where(m => m.Active == true && sale_dep.Any(n => m.DepartmentId.Contains(n)) && m.MemberNumber.Contains(cMem.MemberNumber)).ToList();
            //}

            if (command == "create-trial")
            {
                sl.L_Version = "Trial";
                sl.L_CreateTrialBy = cMem.FullName;
                sl.L_CreateTrialAt = DateTime.UtcNow;
            }
            sl.L_MoreInfo = JsonConvert.SerializeObject(model.MoreInfo);
            if (!string.IsNullOrEmpty(model.Note))
            {
                sl.LastNote = model.Note;
                sl.LastNoteAt = DateTime.UtcNow;
            }
            db.SaveChanges();
            switch (command)
            {
                //action create trial account
                case "create-trial":
                    result = await _salesLeadService.CreateNewRegisterData(Id: sl.Id, phone: sl.L_Phone);
                    await _salesLeadService.SendMailVerify(email: sl.L_Email, name: sl.L_ContactName, phone: sl.L_Phone, link: WebConfigurationManager.AppSettings["HostRegisterMango"] + "/verify?key=" + SecurityLibrary.Encrypt(sl.L_Email));
                    _salesLeadService.CreateLog(SalesLeadId: sl.Id, SalesLeadName: sl.L_SalonName, title: "The trial account is ready to be initiated.<br/> Email verify of the account was sent.", description: model.Note, MemberNumber: cMem.MemberNumber);
                    result = Tuple.Create(true, "This trial account is ready for activation, waiting for authentication via email from customers");
                    break;
                // action add to sale lead
                case "edit-to-sales-lead":
                    result = await _salesLeadService.CreateNewRegisterData(Id: sl.Id, phone: sl.L_Phone);
                    if (result.Item1 == true)
                    {
                        _salesLeadService.CreateLog(SalesLeadId: sl.Id, SalesLeadName: sl.L_SalonName, title: "Data is added to the Sales Lead successfully", description: model.Note, MemberNumber: cMem.MemberNumber);
                    }
                    break;
                //  action verify
                case "verify":
                    if (sl.SL_StatusName.Equals(LeadStatus.SliceAccount.Text()))
                        result = Tuple.Create(true, WebConfigurationManager.AppSettings["HostSliceMango"] + "/verify?key=" + SecurityLibrary.Encrypt(sl.L_Email));
                    else
                        result = Tuple.Create(true, WebConfigurationManager.AppSettings["HostRegisterMango"] + "/verify?key=" + SecurityLibrary.Encrypt(sl.L_Email));
                    break;
                //    action resend email
                case "resend-email":
                    result = await _salesLeadService.CreateNewRegisterData(Id: sl.Id, phone: sl.L_Phone);
                    await _salesLeadService.SendMailVerify(email: sl.L_Email, name: sl.L_ContactName, phone: sl.L_Phone, link: WebConfigurationManager.AppSettings["HostRegisterMango"] + "/verify?key=" + SecurityLibrary.Encrypt(sl.L_Email));
                    _salesLeadService.CreateLog(SalesLeadId: sl.Id, SalesLeadName: sl.L_SalonName, title: "Resend email verify success", description: model.Note, MemberNumber: cMem.MemberNumber);
                    result = Tuple.Create(true, "Resend email verify success");
                    break;

                // action saves
                default:
                    _salesLeadService.CreateLog(SalesLeadId: sl.Id, SalesLeadName: sl.L_SalonName, title: "Update success", description: model.Note, MemberNumber: cMem.MemberNumber);
                    result = Tuple.Create(true, "Update success");
                    break;
            }
            //everything is ok
            if (result.Item1 == false)
            {
                return Json(new { status = false, message = result.Item2, SalesLeadId = sl.Id });
            }
            return Json(new { status = true, message = result.Item2, SalesLeadId = sl.Id });
        }
        [HttpPost]
        public JsonResult NewRegister_Delete(string id)
        {
            try
            {
                var sl = db.C_SalesLead.Where(x => x.Id == id).FirstOrDefault();
                db.C_SalesLead.Remove(sl);
                db.SaveChanges();
                return Json(new { status = true });
            }
            catch
            {
                return Json(new { status = false, message = "An error has occurred, please try again later !" });
            }
        }

        #endregion
        #region Import Data Tab
        //[HttpPost]
        //public ActionResult LoadState_ReportData()
        //{
        //    string TypeImportData = LeadType.ImportData.Text();
        //    int statusMerchant = LeadStatus.Merchant.Code<int>();
        //    int statusLead = LeadStatus.Lead.Code<int>();
        //    int statusTrial = LeadStatus.TrialAccount.Code<int>();
        //    int statusSlice = LeadStatus.SliceAccount.Code<int>();
        //    var ListState = new List<StateSearchSalesLeadModel>();
        //    bool showOtherState = false;
        //    var cusQuery = from sl in db.C_SalesLead where string.IsNullOrEmpty(sl.CustomerCode) && sl.L_Type == TypeImportData select sl;
        //    var queryState = from states in db.Ad_USAState select states;
        //    List<string> listState = new List<string>();
        //    List<string> listMemberNumber = new List<string>();
        //    List<long> listTeam = new List<long>();
        //    int totalRecord = 0;

        //    if ((access.Any(k => k.Key.Equals("sales_lead_view_all")) == true && access["sales_lead_view_all"] == true))
        //    {
        //        var query = from state in queryState
        //                    join cus in cusQuery.GroupBy(x => x.L_State) on state.abbreviation.Trim().ToLower() equals cus.Key.Trim().ToLower() into gj
        //                    from j in gj.DefaultIfEmpty()
        //                    select new StateSearchSalesLeadModel { Code = state.abbreviation, Name = state.name, Number = j.Key != null ? j.Count() : 0 };
        //        ListState.AddRange(query.ToList());
        //        totalRecord = query.Count();
        //        ListState.AddRange(query.ToList());
        //        showOtherState = true;
        //    }
        //    else if (access.Any(k => k.Key.Equals("sales_lead_view_team")) == true && access["sales_lead_view_team"] == true)
        //    {
        //        var ManagerDep = db.P_Department.Where(x => x.SupervisorNumber == cMem.MemberNumber && x.ParentDepartmentId != null && x.Type == "SALES").ToList();
        //        if (ManagerDep.Count() > 0)
        //        {
        //            foreach (var dep in ManagerDep)
        //            {
        //                listState.AddRange(dep.SaleStates.Split(','));
        //                listMemberNumber.AddRange(dep.GroupMemberNumber.Split(','));
        //            }
        //        }
        //        var currentDeparmentsUser = db.P_Department.Where(d => d.Type == "SALES" && d.GroupMemberNumber.Contains(cMem.MemberNumber));
        //        if (currentDeparmentsUser.Count() > 0)
        //        {
        //            foreach (var deparment in currentDeparmentsUser)
        //            {
        //                listState.AddRange(deparment.SaleStates.Split(','));
        //                listTeam.Add(deparment.Id);
        //                if (deparment.LeaderNumber == cMem.MemberNumber)
        //                {
        //                    listMemberNumber.AddRange(deparment.GroupMemberNumber.Split(','));
        //                }
        //            }
        //        }
        //        listMemberNumber.Add(cMem.MemberNumber);
        //        listMemberNumber = listMemberNumber.GroupBy(x => x).Select(x => x.Key).ToList();
        //        queryState = queryState.Where(x => listState.Any(y => y.Contains(x.abbreviation)));
        //        if (listState.Contains("Other"))
        //        {
        //            showOtherState = true;
        //        }
        //        var query = from state in queryState
        //                    join cus in cusQuery.Where(x => listMemberNumber.Any(s => s.Contains(x.MemberNumber) || (string.IsNullOrEmpty(x.MemberNumber) && listTeam.Any(y => y == x.TeamNumber)) || string.IsNullOrEmpty(x.MemberNumber))).GroupBy(x => x.L_State) on state.abbreviation.Trim().ToLower() equals cus.Key.Trim().ToLower() into gj
        //                    from j in gj.DefaultIfEmpty()
        //                    select new StateSearchSalesLeadModel { Code = state.abbreviation, Name = state.name, Number = j.Key != null ? j.Count() : 0 };
        //        ListState.AddRange(query.ToList());
        //        totalRecord = query.Count();
        //    }
        //    return Json(new
        //    {
        //        showOtherState,
        //        data = ListState,
        //        recordsTotal = totalRecord
        //    });
        //}
        //[HttpPost]
        //public ActionResult ImportData_LoadList(IDataTablesRequest dataTableParam, string SearchText, DateTime? FromDate, DateTime? ToDate, string State)
        //{
        //    string TypeImportData = LeadType.ImportData.Text();
        //    var data = from sl in db.C_SalesLead where string.IsNullOrEmpty(sl.CustomerCode) && sl.L_Type == TypeImportData
        //               join mb in db.P_Member on sl.MemberNumber equals mb.MemberNumber into gr
        //               from mb in gr.DefaultIfEmpty()
        //               join dp in db.P_Department on sl.TeamNumber equals dp.Id into grs
        //               from dp in grs.DefaultIfEmpty()
        //               select new { sl, mb, dp };
        //    if (FromDate != null)
        //    {
        //        data = data.Where(x => DbFunctions.TruncateTime(x.sl.CreateAt) >= FromDate);
        //    }
        //    if (ToDate != null)
        //    {
        //        data = data.Where(x => DbFunctions.TruncateTime(x.sl.CreateAt) <= ToDate);
        //    }
        //    if (!string.IsNullOrEmpty(SearchText))
        //    {
        //        data = data.Where(x => x.sl.L_ContactName.Contains(SearchText.Trim()) || x.sl.L_Phone.Contains(SearchText.Trim()) || (x.sl.L_Email.Contains(SearchText.Trim()) || x.sl.L_SalonName.Contains(SearchText.Trim())));
        //    }
        //    //filter data state
        //    if (!string.IsNullOrEmpty(State))
        //    {
        //        if (State.Contains("Other"))
        //        {
        //            var ListAllState = db.Ad_USAState.Select(x => x.abbreviation).ToArray();
        //            data = data.Where(x => State.Contains(x.sl.L_State) || !ListAllState.Contains(x.sl.L_State) || string.IsNullOrEmpty(x.sl.L_State));
        //        }
        //        else
        //        {
        //            data = data.Where(x => State.Contains(x.sl.L_State));
        //        }
        //    }
        //    var currentDeparmentsUser = db.P_Department.Where(d => d.Type == "SALES" && d.GroupMemberNumber.Contains(cMem.MemberNumber));
        //    if (!AppFunc.CanAccess("sales_lead_view_all"))
        //    {
        //        if (AppFunc.CanAccess("sales_lead_view_team"))
        //        {
        //            List<string> listMemberNumber = new List<string>();
        //            List<string> listState = new List<string>();
        //            List<long> listTeam = new List<long>();
        //            var ManagerDep = db.P_Department.Where(x => x.SupervisorNumber == cMem.MemberNumber && x.ParentDepartmentId != null && x.Type == "SALES").ToList();
        //            if (ManagerDep.Count() > 0)
        //            {
        //                foreach (var dep in ManagerDep)
        //                {
        //                    listState.AddRange(dep.SaleStates.Split(','));
        //                    listMemberNumber.AddRange(dep.GroupMemberNumber.Split(','));
        //                }
        //            }
        //            if (currentDeparmentsUser.Count() > 0)
        //            {
        //                foreach (var deparment in currentDeparmentsUser)
        //                {
        //                    listState.AddRange(deparment.SaleStates.Split(','));
        //                    listTeam.Add(deparment.Id);
        //                    if (deparment.LeaderNumber == cMem.MemberNumber)
        //                    {
        //                        listMemberNumber.AddRange(deparment.GroupMemberNumber.Split(','));
        //                    }
        //                }
        //            }
        //            listMemberNumber.Add(cMem.MemberNumber);
        //            if (listState.Count > 0)
        //            {
        //                if (listState.Contains("Other"))
        //                {
        //                    var ListAllState = db.Ad_USAState.Select(x => x.abbreviation).ToArray();
        //                    data = data.Where(x => listMemberNumber.Any(y => y.Contains(x.sl.MemberNumber)) || (string.IsNullOrEmpty(x.sl.MemberNumber) && listTeam.Any(y => y == (x.sl.TeamNumber))) || ((!ListAllState.Contains(x.sl.L_State) || string.IsNullOrEmpty(x.sl.L_State) || listState.Any(s => s.Contains(x.sl.L_State))) && string.IsNullOrEmpty(x.sl.MemberNumber) && x.sl.TeamNumber == null));
        //                }
        //                else
        //                {
        //                    data = data.Where(x => listMemberNumber.Any(y => y.Contains(x.sl.MemberNumber)) || (string.IsNullOrEmpty(x.sl.MemberNumber) && listTeam.Any(y => y == (x.sl.TeamNumber))) || (listState.Any(s => s.Contains(x.sl.L_State)) && string.IsNullOrEmpty(x.sl.MemberNumber) && x.sl.TeamNumber == null));
        //                }

        //            }
        //            else
        //            {
        //                data = data.Where(x => listMemberNumber.Any(y => y.Contains(x.sl.MemberNumber)) || (string.IsNullOrEmpty(x.sl.MemberNumber) && listTeam.Any(y => y == (x.sl.TeamNumber))));
        //            }

        //        }
        //    }
        //    var totalRecord = data.Count();

        //    var colSearch = dataTableParam.Columns.Where(c => c.Sort != null).FirstOrDefault();
        //    switch (colSearch.Name)
        //    {
        //        case "CreateAt":
        //            if (colSearch.Sort.Direction == SortDirection.Ascending)
        //            {
        //                data = data.OrderBy(s => s.sl.CreateAt);
        //            }
        //            else
        //            {
        //                data = data.OrderByDescending(s => s.sl.CreateAt);
        //            }

        //            break;
        //        case "L_Version":
        //            if (colSearch.Sort.Direction == SortDirection.Ascending)
        //            {
        //                data = data.OrderBy(s => s.sl.L_Version);
        //            }
        //            else
        //            {
        //                data = data.OrderByDescending(s => s.sl.L_Version);
        //            }
        //            break;
        //        case "L_State":
        //            if (colSearch.Sort.Direction == SortDirection.Ascending)
        //            {
        //                data = data.OrderBy(s => s.sl.L_State);
        //            }
        //            else
        //            {
        //                data = data.OrderByDescending(s => s.sl.L_State);
        //            }
        //            break;
        //        default:
        //            data = data.OrderByDescending(s => s.sl.CreateAt);
        //            break;
        //    }

        //    data = data.Skip(dataTableParam.Start).Take(dataTableParam.Length);//.SortAndPage(dataTableParam);
        //    try
        //    {
        //        var ViewData = data.ToList().Select(x => new
        //        {
        //            Id = x.sl.Id,
        //            L_ContactName = x.sl.L_ContactName,
        //            L_SalonName = x.sl.L_SalonName ?? string.Empty,
        //            L_ContactPhone = x.sl.L_ContactPhone,
        //            L_Phone = x.sl.L_Phone,
        //            L_Email = x.sl.L_Email,
        //            L_State = x.sl.L_State,
        //            L_Product = x.sl.L_Product ?? "N/A",
        //            VerifyUrl = WebConfigurationManager.AppSettings["HostRegisterMango"] + "/verify?key=" + SecurityLibrary.Encrypt(x.sl.L_Email),
        //            TimeSendMail = x.sl.L_CreateTrialAt != null ? CommonFunc.DateTimeRemain(x.sl.L_CreateTrialAt.GetValueOrDefault()) : null,
        //            StatusVerify = x.sl.L_IsSendMail != null ? x.sl.L_IsSendMail.GetValueOrDefault() : false,
        //            CreateAt = x.sl.CreateAt != null ? x.sl.CreateAt.GetValueOrDefault().ToString("dd/MM/yyyy HH:mm:ss tt") : "",
        //            L_Version = x.sl.L_Version,
        //            AssignedSalesPerson = x.mb?.FullName ?? "N/A",
        //            AssignedTeam = x.dp?.Name ?? "N/A",
        //            Color = x.mb?.ProfileDefinedColor ?? "#ffffff",
        //        });
        //        return Json(new
        //        {
        //            recordsTotal = totalRecord,
        //            recordsFiltered = totalRecord,
        //            draw = dataTableParam.Draw,
        //            data = ViewData
        //        });
        //    }
        //    catch (Exception e)
        //    {
        //        //       System.Diagnostics.Debug.WriteLine(e.Message);
        //        return Json(new
        //        {
        //            recordsTotal = 0,
        //            recordsFiltered = 0,
        //            draw = dataTableParam.Draw,
        //            data = new List<C_SalesLead>()
        //        });
        //    }
        //}
        #endregion
        #region Report Tab 
        [HttpPost]
        public ActionResult ChartTrialAccountAndMerchant(string time, int? siteId, string Team, string salesPerson)
        {
            int LeadSTT = LeadStatus.Lead.Code<int>();
            int TrialAccountSTT = LeadStatus.TrialAccount.Code<int>();
            int SliceAccountSTT = LeadStatus.SliceAccount.Code<int>();
            int MerchantSTT = LeadStatus.Merchant.Code<int>();
            DateTime currentTimeNow = DateTime.UtcNow;
            string title;
            var query = from cus in db.C_Customer join sl in db.C_SalesLead on cus.CustomerCode equals sl.CustomerCode select new { sl, cus };
            ////filter data state
            //if (!string.IsNullOrEmpty(Team))
            //{
            //    List<string> listMemberNumber = new List<string>();
            //    List<string> listState = new List<string>();
            //    var TeamFilter = db.P_Department.Find(long.Parse(Team));
            //    listState.AddRange(TeamFilter.SaleStates.Split(','));
            //    listMemberNumber.AddRange(TeamFilter.GroupMemberNumber.Split(','));
            //    if (listState.Count > 0)
            //    {
            //        if (listState.Contains("Other"))
            //        {
            //            var ListAllState = db.Ad_USAState.Select(x => x.abbreviation).ToArray();
            //            query = query.Where(x => listMemberNumber.Any(y => y.Contains(x.sl.MemberNumber)) || !ListAllState.Contains(x.sl.L_State)|| string.IsNullOrEmpty(x.sl.L_State) || (listState.Any(s => s.Contains(x.cus.BusinessState)) && string.IsNullOrEmpty(x.sl.MemberNumber)));
            //        }
            //        else
            //        {
            //            query = query.Where(x => listMemberNumber.Any(y => y.Contains(x.sl.MemberNumber)) || (listState.Any(s => s.Contains(x.cus.BusinessState)) && string.IsNullOrEmpty(x.sl.MemberNumber)));
            //        }
            //    }
            //    else
            //    {
            //        query = query.Where(x => listMemberNumber.Any(y => y.Contains(x.sl.MemberNumber)));
            //    }
            //}
            //view all sales lead if permission is view all
            //hard code for siteId
            if (cMem.SiteId == 1)
            {
                if (siteId != null)
                {
                    query = query.Where(x => x.cus.SiteId == siteId);
                }
            }
            else
            {
                query = query.Where(x => x.cus.SiteId == cMem.SiteId);
            }
            if (access.Any(k => k.Key.Equals("sales_lead_view_all")) == true && access["sales_lead_view_all"] == true)
            {
                if (!string.IsNullOrEmpty(salesPerson))
                {
                    if (salesPerson == "Unassigned")
                    {
                        query = query.Where(x => string.IsNullOrEmpty(x.sl.MemberNumber));
                    }
                    else
                    {
                        query = query.Where(x => x.sl != null && x.sl.MemberNumber == salesPerson);
                    }
                }
                if (!string.IsNullOrEmpty(Team))
                {
                    List<string> listMemberNumber = new List<string>();
                    List<string> listState = new List<string>();
                    long TeamId = long.Parse(Team);
                    var TeamFilter = db.P_Department.Find(long.Parse(Team));

                    if (TeamFilter.SaleStates != null)
                        listState.AddRange(TeamFilter.SaleStates.Split(','));
                    listMemberNumber.AddRange(TeamFilter.GroupMemberNumber.Split(','));
                    if (listState.Count > 0)
                    {
                        if (listState.Contains("Other"))
                        {
                            var ListAllState = db.Ad_USAState.Select(x => x.abbreviation).ToArray();
                            query = query.Where(x => listMemberNumber.Any(y => y.Contains(x.sl.MemberNumber)) || (string.IsNullOrEmpty(x.sl.MemberNumber) && x.sl.TeamNumber == TeamId) || ((!ListAllState.Any(y => y.Contains(x.sl.L_State)) || string.IsNullOrEmpty(x.sl.L_State) || listState.Any(s => s.Contains(x.cus.BusinessState))) && string.IsNullOrEmpty(x.sl.MemberNumber) && x.sl.TeamNumber == null));
                        }
                        else
                        {
                            query = query.Where(x => listMemberNumber.Any(y => y.Contains(x.sl.MemberNumber)) || (string.IsNullOrEmpty(x.sl.MemberNumber) && x.sl.TeamNumber == TeamId) || (listState.Any(s => s.Contains(x.cus.BusinessState)) && string.IsNullOrEmpty(x.sl.MemberNumber) && x.sl.TeamNumber == null));
                        }
                    }
                    else
                    {
                        query = query.Where(x => listMemberNumber.Any(y => y.Contains(x.sl.MemberNumber)) || (string.IsNullOrEmpty(x.sl.MemberNumber) && x.sl.TeamNumber == TeamId));
                    }
                }
            }
            else if (access.Any(k => k.Key.Equals("sales_lead_view_team")) == true && access["sales_lead_view_team"] == true)
            {

                List<string> listState = new List<string>();
                List<string> listMemberNumber = new List<string>();
                List<long> listTeam = new List<long>();
                var ManagerDep = db.P_Department.Where(x => x.SupervisorNumber == cMem.MemberNumber && x.ParentDepartmentId != null && x.Type == "SALES").ToList();
                if (ManagerDep.Count() > 0)
                {
                    foreach (var dep in ManagerDep)
                    {
                        listState.AddRange(dep.SaleStates.Split(','));
                        listMemberNumber.AddRange(dep.GroupMemberNumber.Split(','));
                    }
                }
                var currentDeparmentsUser = db.P_Department.Where(d => d.Type == "SALES" && d.GroupMemberNumber.Contains(cMem.MemberNumber));
                if (currentDeparmentsUser.Count() > 0)
                {
                    foreach (var deparment in currentDeparmentsUser)
                    {
                        listTeam.Add(deparment.Id);
                        listState.AddRange(deparment.SaleStates.Split(','));
                        if (deparment.LeaderNumber == cMem.MemberNumber)
                        {
                            listMemberNumber.AddRange(deparment.GroupMemberNumber.Split(','));
                        }
                    }
                }
                listMemberNumber.Add(cMem.MemberNumber);
                listMemberNumber = listMemberNumber.GroupBy(x => x).Select(x => x.Key).ToList();
                listState = listState.GroupBy(x => x).Select(x => x.Key).ToList();
                if (listState.Count > 0)
                {
                    if (listState.Contains("Other"))
                    {
                        var ListAllState = db.Ad_USAState.Select(x => x.abbreviation).ToArray();
                        query = query.Where(x => listMemberNumber.Any(y => y.Contains(x.sl.MemberNumber)) || (string.IsNullOrEmpty(x.sl.MemberNumber) && listTeam.Any(y => y == x.sl.TeamNumber)) || ((!ListAllState.Contains(x.sl.L_State) || string.IsNullOrEmpty(x.sl.L_State) || listState.Any(s => s.Contains(x.cus.BusinessState))) && string.IsNullOrEmpty(x.sl.MemberNumber) && x.sl.TeamNumber == null));
                    }
                    else
                    {
                        query = query.Where(x => listMemberNumber.Any(y => y.Contains(x.sl.MemberNumber)) || (string.IsNullOrEmpty(x.sl.MemberNumber) && listTeam.Any(y => y == x.sl.TeamNumber)) || (listState.Any(s => s.Contains(x.cus.BusinessState)) && string.IsNullOrEmpty(x.sl.MemberNumber) && x.sl.TeamNumber == null));
                    }
                }
                else
                {
                    query = query.Where(x => listMemberNumber.Any(y => y.Contains(x.sl.MemberNumber) || (string.IsNullOrEmpty(x.sl.MemberNumber) && listTeam.Any(my => my == x.sl.TeamNumber))));
                }
                if (!string.IsNullOrEmpty(salesPerson))
                {
                    if (salesPerson == "Unassigned")
                    {
                        query = query.Where(x => string.IsNullOrEmpty(x.sl.MemberNumber));
                    }
                    else
                    {
                        query = query.Where(x => x.sl != null && x.sl.MemberNumber == salesPerson);
                    }

                }
            }
            switch (time)
            {
                //this month
                case "current-month":
                    title = "Customer statistics in this month";
                    query = query.Where(x => x.cus.CreateAt.Value.Month == currentTimeNow.Month && x.cus.CreateAt.Value.Year == currentTimeNow.Year);
                    var RangeNumber = Enumerable.Range(1, currentTimeNow.Day);
                    var label = new string[RangeNumber.Count()];

                    for (int i = 0; i < RangeNumber.Count(); i++)
                    {
                        label[i] = new DateTime(currentTimeNow.Year, currentTimeNow.Month, i + 1).ToString("dd ddd");
                    }
                    var SampleWithEmptyData = new int[label.Count()];
                    var data = query.GroupBy(d => d.sl.SL_Status)
                       .Select(g =>
                               new
                               {
                                   Name = g.Key,
                                   Data = (
                                   from m in RangeNumber
                                   join d in
                                       g.OrderBy(gg => gg.cus.CreateAt.Value.Day)
                                       .GroupBy(gg => gg.cus.CreateAt.Value.Day)
                                       on m equals d.Key into gj
                                   from j in gj.DefaultIfEmpty()
                                   select j.Key != null ? j.Count() : 0)
                               });
                    var Data = data.Count() > 0 ? data.ToArray() : null;
                    var maxY = query.Count();
                    return Json(new { Data, SampleWithEmptyData, maxY, title, label });
                //last month
                case "last-month":
                    currentTimeNow = currentTimeNow.AddMonths(-1);
                    query = query.Where(x => x.cus.CreateAt.Value.Month == currentTimeNow.Month && x.cus.CreateAt.Value.Year == currentTimeNow.Year);
                    title = "Customer statistics in last month";
                    RangeNumber = Enumerable.Range(1, DateTime.DaysInMonth(currentTimeNow.Year, currentTimeNow.Month));
                    label = new string[RangeNumber.Count()];

                    for (int i = 0; i < RangeNumber.Count(); i++)
                    {
                        label[i] = new DateTime(currentTimeNow.Year, currentTimeNow.Month, i + 1).ToString("dd ddd");
                    }
                    data = query.GroupBy(d => d.sl.SL_Status)
                            .Select(g =>
                                    new
                                    {
                                        Name = g.Key,
                                        Data = (
                                        from m in RangeNumber
                                        join d in
                                            g.OrderBy(gg => gg.cus.CreateAt.Value.Day)
                                            .GroupBy(gg => gg.cus.CreateAt.Value.Day)
                                            on m equals d.Key into gj
                                        from j in gj.DefaultIfEmpty()
                                        select j.Key != null ? j.Count() : 0)
                                    });
                    maxY = query.Count();
                    Data = data.Count() > 0 ? data.ToArray() : null;
                    return Json(new { Data, maxY, title, label });
                case "nearest-3-months":
                    currentTimeNow = new DateTime(currentTimeNow.Year, currentTimeNow.Month, 1).AddMonths(-2);
                    query = query.Where(x => DbFunctions.TruncateTime(x.cus.CreateAt) >= currentTimeNow);
                    var ThreeMonth = new int[3] { DateTime.UtcNow.Month, DateTime.UtcNow.AddMonths(-1).Month, DateTime.UtcNow.AddMonths(-2).Month };
                    label = new string[3];
                    for (int i = 0; i < 3; i++)
                    {
                        label[i] = DateTime.UtcNow.AddMonths(-(i)).ToString("MMMM yyyy");
                    }
                    title = "Customer statistics in Nearest 3 months";
                    data = query.GroupBy(d => d.sl.SL_Status)
                    .Select(g =>
                            new
                            {
                                Name = g.Key,
                                Data = (
                                from m in ThreeMonth
                                join d in
                                    g.OrderBy(gg => gg.cus.CreateAt.Value.Month)
                                    .GroupBy(gg => gg.cus.CreateAt.Value.Month)
                                    on m equals d.Key into gj
                                from j in gj.DefaultIfEmpty()
                                select j.Key != null ? j.Count() : 0)
                            });
                    maxY = query.Count();
                    Data = data.Count() > 0 ? data.ToArray() : null;
                    return Json(new { Data, maxY, title, label });
                case "current-year":
                    query = query.Where(x => x.cus.CreateAt.Value.Year == currentTimeNow.Year);
                    RangeNumber = Enumerable.Range(1, currentTimeNow.Month);
                    label = new string[RangeNumber.Count()];
                    for (int i = 0; i < RangeNumber.Count(); i++)
                    {
                        label[i] = new DateTime(currentTimeNow.Year, i + 1, 1).ToString("MMMM yyyy");
                    }
                    title = "Customer statistics in this year";
                    data = query.GroupBy(d => d.sl.SL_Status)
                    .Select(g =>
                            new
                            {
                                Name = g.Key,
                                Data = (
                                from m in RangeNumber
                                join d in
                                    g.OrderBy(gg => gg.cus.CreateAt.Value.Month)
                                    .GroupBy(gg => gg.cus.CreateAt.Value.Month)
                                    on m equals d.Key into gj
                                from j in gj.DefaultIfEmpty()
                                select j.Key != null ? j.Count() : 0)
                            });
                    maxY = query.Count();
                    Data = data.Count() > 0 ? data.ToArray() : null;
                    return Json(new { Data, maxY, title, label });
                case "last-year":
                    currentTimeNow = currentTimeNow.AddYears(-1);
                    RangeNumber = Enumerable.Range(1, 12);
                    label = new string[RangeNumber.Count()];
                    for (int i = 0; i < RangeNumber.Count(); i++)
                    {
                        label[i] = new DateTime(currentTimeNow.Year, i + 1, 1).ToString("MMMM yyyy");
                    }
                    query = query.Where(x => x.cus.CreateAt.Value.Year == currentTimeNow.Year);
                    title = "Customer statistics in last year";
                    data = query.GroupBy(d => d.sl.SL_Status)
                    .Select(g =>
                            new
                            {
                                Name = g.Key,
                                Data = (
                                from m in RangeNumber
                                join d in
                                    g.OrderBy(gg => gg.cus.CreateAt.Value.Month)
                                    .GroupBy(gg => gg.cus.CreateAt.Value.Month)
                                    on m equals d.Key into gj
                                from j in gj.DefaultIfEmpty()
                                select j.Key != null ? j.Count() : 0)
                            });
                    maxY = query.Count();
                    Data = data.Count() > 0 ? data.ToArray() : null;
                    return Json(new { Data, maxY, title, label });
                default:
                    throw new Exception("Time Report Incorrect");
            }
        }
        [HttpPost]
        public ActionResult GetDashboardReportSalesLead(string time, int? siteId, string Team, string salesPerson)
        {
            string RegisterOnIMSType = LeadType.RegisterOnIMS.Text();
            string SubscribeMangoType = LeadType.SubscribeMango.Text();
            string TrialAccountType = LeadType.TrialAccount.Text();
            string CreateBySaler = LeadType.CreateBySaler.Text();
            string ImportDataType = LeadType.ImportData.Text();
            string SliceAccount = LeadType.SliceAccount.Text();
            int LeadSTT = LeadStatus.Lead.Code<int>();
            int TrialAccountSTT = LeadStatus.TrialAccount.Code<int>();
            int SliceAccountSTT = LeadStatus.SliceAccount.Code<int>();
            int MerchantSTT = LeadStatus.Merchant.Code<int>();
            var query = from sl in db.C_SalesLead join cus in db.C_Customer on sl.CustomerCode equals cus.CustomerCode select new { sl, cus };
            ////filter data state
            //if (!string.IsNullOrEmpty(Team))
            //{
            //    List<string> listMemberNumber = new List<string>();
            //    List<string> listState = new List<string>();
            //    var TeamFilter = db.P_Department.Find(long.Parse(Team));
            //    listState.AddRange(TeamFilter.SaleStates.Split(','));
            //    listMemberNumber.AddRange(TeamFilter.GroupMemberNumber.Split(','));
            //    if (listState.Count > 0)
            //    {
            //        if (listState.Contains("Other"))
            //        {
            //            var ListAllState = db.Ad_USAState.Select(x => x.abbreviation).ToArray();
            //            query = query.Where(x => listMemberNumber.Any(y => y.Contains(x.sl.MemberNumber))|| !ListAllState.Contains(x.sl.L_State) || string.IsNullOrEmpty(x.sl.L_State) || (listState.Any(s => s.Contains(x.cus.BusinessState)) && string.IsNullOrEmpty(x.sl.MemberNumber)));
            //        }
            //        else
            //        {
            //            query = query.Where(x => listMemberNumber.Any(y => y.Contains(x.sl.MemberNumber)) || (listState.Any(s => s.Contains(x.cus.BusinessState)) && string.IsNullOrEmpty(x.sl.MemberNumber)));
            //        }
            //    }
            //    else
            //    {
            //        query = query.Where(x => listMemberNumber.Any(y => y.Contains(x.sl.MemberNumber)));
            //    }
            //}
            if (cMem.SiteId == 1)
            {
                if (siteId != null)
                {
                    query = query.Where(x => x.cus.SiteId == siteId);
                }
            }
            else
            {
                query = query.Where(x => x.cus.SiteId == cMem.SiteId);
            }
            if (access.Any(k => k.Key.Equals("sales_lead_view_all")) == true && access["sales_lead_view_all"] == true)
            {
                if (!string.IsNullOrEmpty(salesPerson))
                {
                    if (salesPerson == "Unassigned")
                    {
                        query = query.Where(x => string.IsNullOrEmpty(x.sl.MemberNumber));
                    }
                    else
                    {
                        query = query.Where(x => x.sl != null && x.sl.MemberNumber == salesPerson);
                    }
                }
                if (!string.IsNullOrEmpty(Team))
                {
                    List<string> listMemberNumber = new List<string>();
                    List<string> listState = new List<string>();
                    long TeamId = long.Parse(Team);
                    var TeamFilter = db.P_Department.Find(long.Parse(Team));

                    if (TeamFilter.SaleStates != null)
                        listState.AddRange(TeamFilter.SaleStates.Split(','));
                    listMemberNumber.AddRange(TeamFilter.GroupMemberNumber.Split(','));
                    if (listState.Count > 0)
                    {
                        if (listState.Contains("Other"))
                        {
                            var ListAllState = db.Ad_USAState.Select(x => x.abbreviation).ToArray();
                            query = query.Where(x => listMemberNumber.Any(y => y.Contains(x.sl.MemberNumber)) || (string.IsNullOrEmpty(x.sl.MemberNumber) && x.sl.TeamNumber == TeamId) || ((!ListAllState.Any(y => y.Contains(x.sl.L_State)) || string.IsNullOrEmpty(x.sl.L_State) || listState.Any(s => s.Contains(x.cus.BusinessState))) && string.IsNullOrEmpty(x.sl.MemberNumber) && x.sl.TeamNumber == null));
                        }
                        else
                        {
                            query = query.Where(x => listMemberNumber.Any(y => y.Contains(x.sl.MemberNumber)) || (string.IsNullOrEmpty(x.sl.MemberNumber) && x.sl.TeamNumber == TeamId) || (listState.Any(s => s.Contains(x.cus.BusinessState)) && string.IsNullOrEmpty(x.sl.MemberNumber) && x.sl.TeamNumber == null));
                        }
                    }
                    else
                    {
                        query = query.Where(x => listMemberNumber.Any(y => y.Contains(x.sl.MemberNumber)) || (string.IsNullOrEmpty(x.sl.MemberNumber) && x.sl.TeamNumber == TeamId));
                    }
                }
            }
            else if (access.Any(k => k.Key.Equals("sales_lead_view_team")) == true && access["sales_lead_view_team"] == true)
            {
                List<string> listState = new List<string>();
                List<string> listMemberNumber = new List<string>();
                List<long> listTeam = new List<long>();
                var ManagerDep = db.P_Department.Where(x => x.SupervisorNumber == cMem.MemberNumber && x.ParentDepartmentId != null && x.Type == "SALES").ToList();
                if (ManagerDep.Count() > 0)
                {
                    foreach (var dep in ManagerDep)
                    {
                        listState.AddRange(dep.SaleStates.Split(','));
                        listMemberNumber.AddRange(dep.GroupMemberNumber.Split(','));
                    }
                }
                var currentDeparmentsUser = db.P_Department.Where(d => d.Type == "SALES" && d.GroupMemberNumber.Contains(cMem.MemberNumber));
                if (currentDeparmentsUser.Count() > 0)
                {
                    foreach (var deparment in currentDeparmentsUser)
                    {
                        listTeam.Add(deparment.Id);
                        listState.AddRange(deparment.SaleStates.Split(','));
                        if (deparment.LeaderNumber == cMem.MemberNumber)
                        {
                            listMemberNumber.AddRange(deparment.GroupMemberNumber.Split(','));
                        }
                    }
                }
                listMemberNumber.Add(cMem.MemberNumber);
                listMemberNumber = listMemberNumber.GroupBy(x => x).Select(x => x.Key).ToList();
                listState = listState.GroupBy(x => x).Select(x => x.Key).ToList();
                if (listState.Count > 0)
                {
                    if (listState.Contains("Other"))
                    {
                        var ListAllState = db.Ad_USAState.Select(x => x.abbreviation).ToArray();
                        query = query.Where(x => listMemberNumber.Any(y => y.Contains(x.sl.MemberNumber)) || (string.IsNullOrEmpty(x.sl.MemberNumber) && listTeam.Any(y => y == x.sl.TeamNumber)) || ((!ListAllState.Contains(x.sl.L_State) || string.IsNullOrEmpty(x.sl.L_State) || listState.Any(s => s.Contains(x.cus.BusinessState))) && string.IsNullOrEmpty(x.sl.MemberNumber) && x.sl.TeamNumber == null));
                    }
                    else
                    {
                        query = query.Where(x => listMemberNumber.Any(y => y.Contains(x.sl.MemberNumber)) || (string.IsNullOrEmpty(x.sl.MemberNumber) && listTeam.Any(y => y == x.sl.TeamNumber)) || (listState.Any(s => s.Contains(x.cus.BusinessState)) && string.IsNullOrEmpty(x.sl.MemberNumber) && x.sl.TeamNumber == null));
                    }
                }
                else
                {
                    query = query.Where(x => listMemberNumber.Any(y => y.Contains(x.sl.MemberNumber)) || (string.IsNullOrEmpty(x.sl.MemberNumber) && listTeam.Any(y => y == x.sl.TeamNumber)));
                }
                if (!string.IsNullOrEmpty(salesPerson))
                {
                    if (salesPerson == "Unassigned")
                    {
                        query = query.Where(x => string.IsNullOrEmpty(x.sl.MemberNumber));
                    }
                    else
                    {
                        query = query.Where(x => x.sl != null && x.sl.MemberNumber == salesPerson);
                    }
                }
            }
            DateTime currentTimeNow = DateTime.UtcNow;
            switch (time)
            {
                case "current-month":
                    query = query.Where(x => x.cus.CreateAt.Value.Month == currentTimeNow.Month && x.cus.CreateAt.Value.Year == currentTimeNow.Year);
                    break;
                case "last-month":
                    currentTimeNow = currentTimeNow.AddMonths(-1);
                    query = query.Where(x => x.cus.CreateAt.Value.Month == currentTimeNow.Month && x.cus.CreateAt.Value.Year == currentTimeNow.Year);
                    break;
                case "nearest-3-months":
                    currentTimeNow = new DateTime(currentTimeNow.Year, currentTimeNow.Month, 1).AddMonths(-3);
                    query = query.Where(x => DbFunctions.TruncateTime(x.cus.CreateAt) >= currentTimeNow);
                    break;
                case "current-year":
                    query = query.Where(x => x.cus.CreateAt.Value.Year == currentTimeNow.Year);
                    break;
                case "last-year":
                    currentTimeNow = currentTimeNow.AddYears(-1);
                    query = query.Where(x => x.cus.CreateAt.Value.Year == currentTimeNow.Year);
                    break;
                default:
                    break;
            }

            var AllQuery = query;
            int TotalAll = AllQuery.Count();
            int TotalAllCreateIMS = AllQuery.Where(x => x.sl.L_Type.Trim().ToLower() == CreateBySaler.Trim().ToLower() || x.sl.L_Type.Trim().ToLower() == RegisterOnIMSType.Trim().ToLower()).Count();
            int TotalAllMango = AllQuery.Where(x => x.sl.L_Type == SliceAccount || x.sl.L_Type.Trim().ToLower() == SubscribeMangoType.Trim().ToLower() || x.sl.L_Type.Trim().ToLower() == TrialAccountType.Trim().ToLower()).Count();
            int TotalAllExternal = TotalAll - (TotalAllMango + TotalAllCreateIMS);

            var LeadQuery = query.Where(x => x.sl.SL_Status == LeadSTT);

            int TotalLead = LeadQuery.Count();
            int TotalLeadMango = LeadQuery.Where(x => x.sl.L_Type == SliceAccount || x.sl.L_Type.Trim().ToLower() == SubscribeMangoType.Trim().ToLower() || x.sl.L_Type.Trim().ToLower() == TrialAccountType.Trim().ToLower()).Count();
            int TotalLeadExternal = LeadQuery.Where(x => x.sl.L_Type == ImportDataType).Count();
            int TotalLeadCreateIMS = TotalLead - (TotalLeadMango + TotalLeadExternal);

            var TrialQuery = query.Where(x => x.sl.SL_Status == TrialAccountSTT);
            int TotalTrial = TrialQuery.Count();
            int TotalTrialMango = TrialQuery.Where(x => x.sl.L_Type == SliceAccount || x.sl.L_Type.Trim().ToLower() == SubscribeMangoType.Trim().ToLower() || x.sl.L_Type.Trim().ToLower() == TrialAccountType.Trim().ToLower()).Count();
            int TotalTrialExternal = TrialQuery.Where(x => x.sl.L_Type == ImportDataType).Count();
            int TotalTrialCreateIMS = TotalTrial - (TotalTrialExternal + TotalTrialMango);

            var SliceQuery = query.Where(x => x.sl.SL_Status == SliceAccountSTT);
            int TotalSlice = SliceQuery.Count();
            int TotalSliceMango = SliceQuery.Where(x => x.sl.L_Type == SliceAccount || x.sl.L_Type.Trim().ToLower() == SubscribeMangoType.Trim().ToLower() || x.sl.L_Type.Trim().ToLower() == TrialAccountType.Trim().ToLower()).Count();
            int TotalSliceExternal = SliceQuery.Where(x => x.sl.L_Type == ImportDataType).Count();
            int TotalSliceCreateIMS = TotalSlice - (TotalSliceExternal + TotalSliceMango);

            var MerchantQuery = query.Where(x => x.sl.SL_Status == MerchantSTT);
            int TotalMerchant = MerchantQuery.Count();
            int TotalMerchantMango = MerchantQuery.Where(x => x.sl.L_Type == SliceAccount || x.sl != null && (x.sl.L_Type.Trim().ToLower() == SubscribeMangoType.Trim().ToLower() || x.sl.L_Type.Trim().ToLower() == TrialAccountType.Trim().ToLower())).Count();
            int TotalMerchantExternal = MerchantQuery.Where(x => x.sl.L_Type == ImportDataType).Count();
            int TotalMerchantCreateIMS = TotalMerchant - (TotalMerchantMango + TotalMerchantExternal);
            return Json(new
            {
                TotalAll,
                TotalAllCreateIMS,
                TotalAllMango,
                TotalAllExternal,
                TotalLead,
                TotalLeadCreateIMS,
                TotalLeadMango,
                TotalLeadExternal,
                TotalTrial,
                TotalTrialCreateIMS,
                TotalTrialMango,
                TotalTrialExternal,
                TotalSlice,
                TotalSliceMango,
                TotalSliceExternal,
                TotalSliceCreateIMS,
                TotalMerchant,
                TotalMerchantCreateIMS,
                TotalMerchantMango,
                TotalMerchantExternal
            });
        }
        [HttpPost]
        public ActionResult GetDataPieChart(string time, int? siteId, string Team, string salesPerson)
        {

            string RegisterOnIMSType = LeadType.RegisterOnIMS.Text();
            string CreateBySaler = LeadType.CreateBySaler.Text();
            string SubscribeMangoType = LeadType.SubscribeMango.Text();
            string TrialAccountType = LeadType.TrialAccount.Text();
            string ImportDataType = LeadType.ImportData.Text();
            string SliceAccountDataType = LeadType.SliceAccount.Text();
            int LeadSTT = LeadStatus.Lead.Code<int>();
            int TrialAccountSTT = LeadStatus.TrialAccount.Code<int>();
            int SliceSTT = LeadStatus.SliceAccount.Code<int>();
            int MerchantSTT = LeadStatus.Merchant.Code<int>();
            DateTime currentTimeNow = DateTime.UtcNow;
            var Query = from sl in db.C_SalesLead join cus in db.C_Customer on sl.CustomerCode equals cus.CustomerCode select new { sl, cus };
            if (cMem.SiteId == 1)
            {
                if (siteId != null)
                {
                    Query = Query.Where(x => x.cus.SiteId == siteId);
                }
            }
            else
            {
                Query = Query.Where(x => x.cus.SiteId == cMem.SiteId);
            }
            //filter data state
            if (access.Any(k => k.Key.Equals("sales_lead_view_all")) == true && access["sales_lead_view_all"] == true)
            {
                if (!string.IsNullOrEmpty(salesPerson))
                {
                    if (salesPerson == "Unassigned")
                    {
                        Query = Query.Where(x => string.IsNullOrEmpty(x.sl.MemberNumber));
                    }
                    else
                    {
                        Query = Query.Where(x => x.sl != null && x.sl.MemberNumber == salesPerson);
                    }
                }
                if (!string.IsNullOrEmpty(Team))
                {
                    List<string> listMemberNumber = new List<string>();
                    List<string> listState = new List<string>();
                    long TeamId = long.Parse(Team);
                    var TeamFilter = db.P_Department.Find(long.Parse(Team));

                    if (TeamFilter.SaleStates != null)
                        listState.AddRange(TeamFilter.SaleStates.Split(','));
                    listMemberNumber.AddRange(TeamFilter.GroupMemberNumber.Split(','));
                    if (listState.Count > 0)
                    {
                        if (listState.Contains("Other"))
                        {
                            var ListAllState = db.Ad_USAState.Select(x => x.abbreviation).ToArray();
                            Query = Query.Where(x => listMemberNumber.Any(y => y.Contains(x.sl.MemberNumber)) || (string.IsNullOrEmpty(x.sl.MemberNumber) && x.sl.TeamNumber == TeamId) || ((!ListAllState.Any(y => y.Contains(x.sl.L_State)) || string.IsNullOrEmpty(x.sl.L_State) || listState.Any(s => s.Contains(x.cus.BusinessState))) && string.IsNullOrEmpty(x.sl.MemberNumber) && x.sl.TeamNumber == null));
                        }
                        else
                        {
                            Query = Query.Where(x => listMemberNumber.Any(y => y.Contains(x.sl.MemberNumber)) || (string.IsNullOrEmpty(x.sl.MemberNumber) && x.sl.TeamNumber == TeamId) || (listState.Any(s => s.Contains(x.cus.BusinessState)) && string.IsNullOrEmpty(x.sl.MemberNumber) && x.sl.TeamNumber == null));
                        }
                    }
                    else
                    {
                        Query = Query.Where(x => listMemberNumber.Any(y => y.Contains(x.sl.MemberNumber)) || (string.IsNullOrEmpty(x.sl.MemberNumber) && x.sl.TeamNumber == TeamId));
                    }
                }
            }
            else if (access.Any(k => k.Key.Equals("sales_lead_view_team")) == true && access["sales_lead_view_team"] == true)
            {
                List<string> listState = new List<string>();
                List<string> listMemberNumber = new List<string>();
                List<long> listTeam = new List<long>();
                var ManagerDep = db.P_Department.Where(x => x.SupervisorNumber == cMem.MemberNumber && x.ParentDepartmentId != null && x.Type == "SALES").ToList();
                if (ManagerDep.Count() > 0)
                {
                    foreach (var dep in ManagerDep)
                    {
                        listState.AddRange(dep.SaleStates.Split(','));
                        listMemberNumber.AddRange(dep.GroupMemberNumber.Split(','));
                    }
                }
                var currentDeparmentsUser = db.P_Department.Where(d => d.Type == "SALES" && d.GroupMemberNumber.Contains(cMem.MemberNumber));
                if (currentDeparmentsUser.Count() > 0)
                {
                    foreach (var deparment in currentDeparmentsUser)
                    {
                        listTeam.Add(deparment.Id);
                        listState.AddRange(deparment.SaleStates.Split(','));
                        if (deparment.LeaderNumber == cMem.MemberNumber)
                        {
                            listMemberNumber.AddRange(deparment.GroupMemberNumber.Split(','));
                        }
                    }
                }
                listMemberNumber.Add(cMem.MemberNumber);
                listMemberNumber = listMemberNumber.GroupBy(x => x).Select(x => x.Key).ToList();
                listState = listState.GroupBy(x => x).Select(x => x.Key).ToList();
                if (listState.Count() > 0)
                {
                    if (listState.Contains("Other"))
                    {
                        var ListAllState = db.Ad_USAState.Select(x => x.abbreviation).ToArray();
                        Query = Query.Where(x => listMemberNumber.Any(y => y.Contains(x.sl.MemberNumber)) || (string.IsNullOrEmpty(x.sl.MemberNumber) && listTeam.Any(y => y == x.sl.TeamNumber)) || ((!ListAllState.Contains(x.sl.L_State) || string.IsNullOrEmpty(x.sl.L_State) || listState.Any(s => s.Contains(x.cus.BusinessState))) && string.IsNullOrEmpty(x.sl.MemberNumber) && x.sl.TeamNumber == null));
                    }
                    else
                    {
                        Query = Query.Where(x => listMemberNumber.Any(y => y.Contains(x.sl.MemberNumber)) || (string.IsNullOrEmpty(x.sl.MemberNumber) && listTeam.Any(y => y == x.sl.TeamNumber)) || (listState.Any(s => s.Contains(x.cus.BusinessState)) && string.IsNullOrEmpty(x.sl.MemberNumber) && x.sl.TeamNumber == null));
                    }
                }
                else
                {
                    Query = Query.Where(x => listMemberNumber.Any(y => y.Contains(x.sl.MemberNumber)) || (string.IsNullOrEmpty(x.sl.MemberNumber) && listTeam.Any(y => y == x.sl.TeamNumber)));
                }
                if (!string.IsNullOrEmpty(salesPerson))
                {
                    if (salesPerson == "Unassigned")
                    {
                        Query = Query.Where(x => string.IsNullOrEmpty(x.sl.MemberNumber));
                    }
                    else
                    {
                        Query = Query.Where(x => x.sl != null && x.sl.MemberNumber == salesPerson);
                    }
                }
            }

            switch (time)
            {
                case "current-month":
                    Query = Query.Where(x => x.cus.CreateAt.Value.Month == currentTimeNow.Month && x.cus.CreateAt.Value.Year == currentTimeNow.Year);
                    break;
                case "last-month":
                    currentTimeNow = currentTimeNow.AddMonths(-1);
                    Query = Query.Where(x => x.cus.CreateAt.Value.Month == currentTimeNow.Month && x.cus.CreateAt.Value.Year == currentTimeNow.Year);
                    break;
                case "nearest-3-months":
                    currentTimeNow = new DateTime(currentTimeNow.Year, currentTimeNow.Month, 1).AddMonths(-3);
                    Query = Query.Where(x => DbFunctions.TruncateTime(x.cus.CreateAt) >= currentTimeNow);
                    break;
                case "current-year":
                    Query = Query.Where(x => x.cus.CreateAt.Value.Year == currentTimeNow.Year);
                    break;
                case "last-year":
                    currentTimeNow = currentTimeNow.AddYears(-1);
                    Query = Query.Where(x => x.cus.CreateAt.Value.Year == currentTimeNow.Year);
                    break;
                default:
                    break;
            }
            var QueryCreateFromIMS = Query.Where(x => x.sl.L_Type.Trim().ToLower() == CreateBySaler.Trim().ToLower() || x.sl.L_Type.Trim().ToLower() == RegisterOnIMSType.Trim().ToLower() || string.IsNullOrEmpty(x.sl.L_Type));
            int TotalLeadCreateFromIMS = QueryCreateFromIMS.Where(x => x.sl.SL_Status == LeadSTT).Count();
            int TotalTrialCreateFromIMS = QueryCreateFromIMS.Where(x => x.sl.SL_Status == TrialAccountSTT).Count();
            int TotalSliceCreateFromIMS = QueryCreateFromIMS.Where(x => x.sl.SL_Status == SliceSTT).Count();
            int TotalMerchantCreateFromIMS = QueryCreateFromIMS.Where(x => x.sl.SL_Status == MerchantSTT).Count();
            int[] dataCreateFromIMS = new int[4] { TotalLeadCreateFromIMS, TotalTrialCreateFromIMS, TotalSliceCreateFromIMS, TotalMerchantCreateFromIMS };

            var QueryCreateFromMangoSite = Query.Where(x => x.sl.L_Type.Trim().ToLower() == SubscribeMangoType.Trim().ToLower() || x.sl.L_Type.Trim().ToLower() == TrialAccountType.Trim().ToLower() || x.sl.L_Type.Trim().ToLower() == SliceAccountDataType.Trim().ToLower());
            int TotalLeadCreateFromMangoSite = QueryCreateFromMangoSite.Where(x => x.sl.SL_Status == LeadSTT).Count();
            int TotalTrialCreateFromMangoSite = QueryCreateFromMangoSite.Where(x => x.sl.SL_Status == TrialAccountSTT).Count();
            int TotalSliceCreateFromMangoSite = QueryCreateFromMangoSite.Where(x => x.sl.SL_Status == SliceSTT).Count();
            int TotalMerchantCreateFromMangoSite = QueryCreateFromMangoSite.Where(x => x.sl.SL_Status == MerchantSTT).Count();
            int[] dataCreateFromMangoSite = new int[4] { TotalLeadCreateFromMangoSite, TotalTrialCreateFromMangoSite, TotalSliceCreateFromMangoSite, TotalMerchantCreateFromMangoSite };

            var QueryCreateFromExternal = Query.Where(x => x.sl.L_Type == ImportDataType);
            int TotalLeadCreateFromExternal = QueryCreateFromExternal.Where(x => x.sl.SL_Status == LeadSTT).Count();
            int TotalTrialCreateFromExternal = QueryCreateFromExternal.Where(x => x.sl.SL_Status == TrialAccountSTT).Count();
            int TotalSliceCreateFromExternal = QueryCreateFromExternal.Where(x => x.sl.SL_Status == SliceSTT).Count();
            int TotalMerchantCreateFromExternal = QueryCreateFromExternal.Where(x => x.sl.SL_Status == MerchantSTT).Count();
            int[] dataCreateFromExternal = new int[4] { TotalLeadCreateFromExternal, TotalTrialCreateFromExternal, TotalSliceCreateFromExternal, TotalMerchantCreateFromExternal };
            return Json(new { dataCreateFromIMS, dataCreateFromMangoSite, dataCreateFromExternal }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult PyramidChart(string time, int? siteId, string Team, string salesPerson)
        {
            int LeadSTT = LeadStatus.Lead.Code<int>();
            int TrialAccountSTT = LeadStatus.TrialAccount.Code<int>();
            int SliceAccountSTT = LeadStatus.SliceAccount.Code<int>();
            int MerchantSTT = LeadStatus.Merchant.Code<int>();
            DateTime currentTimeNow = DateTime.UtcNow;
            string title;
            var query = from sl in db.C_SalesLead
                        join cus in db.C_Customer on sl.CustomerCode equals cus.CustomerCode into ps
                        from cus in ps.DefaultIfEmpty()
                        select new { sl, cus };
            if (cMem.SiteId == 1)
            {
                if (siteId != null)
                {
                    query = query.Where(x => x.cus.SiteId == siteId);
                }
            }
            else
            {
                query = query.Where(x => x.cus.SiteId == cMem.SiteId);
            }
            //view all sales lead if permission is view all
            if (access.Any(k => k.Key.Equals("sales_lead_view_all")) == true && access["sales_lead_view_all"] == true)
            {
                if (!string.IsNullOrEmpty(salesPerson))
                {
                    if (salesPerson == "Unassigned")
                    {
                        query = query.Where(x => string.IsNullOrEmpty(x.sl.MemberNumber));
                    }
                    else
                    {
                        query = query.Where(x => x.sl != null && x.sl.MemberNumber == salesPerson);
                    }
                }
            }
            else if (access.Any(k => k.Key.Equals("sales_lead_view_team")) == true && access["sales_lead_view_team"] == true)
            {
                List<string> listState = new List<string>();
                List<string> listMemberNumber = new List<string>();
                List<long> listTeam = new List<long>();
                var ManagerDep = db.P_Department.Where(x => x.SupervisorNumber == cMem.MemberNumber && x.ParentDepartmentId != null && x.Type == "SALES").ToList();
                if (ManagerDep.Count() > 0)
                {
                    foreach (var dep in ManagerDep)
                    {
                        listState.AddRange(dep.SaleStates.Split(','));
                        listMemberNumber.AddRange(dep.GroupMemberNumber.Split(','));
                    }
                }
                var currentDeparmentsUser = db.P_Department.Where(d => d.Type == "SALES" && d.GroupMemberNumber.Contains(cMem.MemberNumber));
                if (currentDeparmentsUser.Count() > 0)
                {
                    foreach (var deparment in currentDeparmentsUser)
                    {
                        listTeam.Add(deparment.Id);
                        listState.AddRange(deparment.SaleStates.Split(','));
                        if (deparment.LeaderNumber == cMem.MemberNumber)
                        {
                            listMemberNumber.AddRange(deparment.GroupMemberNumber.Split(','));
                        }
                    }
                }
                listMemberNumber.Add(cMem.MemberNumber);
                listMemberNumber = listMemberNumber.GroupBy(x => x).Select(x => x.Key).ToList();
                listState = listState.GroupBy(x => x).Select(x => x.Key).ToList();
                if (listState.Count > 0)
                {
                    if (listState.Contains("Other"))
                    {
                        var ListAllState = db.Ad_USAState.Select(x => x.abbreviation).ToArray();
                        query = query.Where(x => listMemberNumber.Any(y => y.Contains(x.sl.MemberNumber)) || (string.IsNullOrEmpty(x.sl.MemberNumber) && listTeam.Any(y => y == x.sl.TeamNumber)) || ((!ListAllState.Contains(x.sl.L_State) || string.IsNullOrEmpty(x.sl.L_State) || listState.Any(s => s.Contains(x.cus.BusinessState))) && string.IsNullOrEmpty(x.sl.MemberNumber) && x.sl.TeamNumber == null));
                    }
                    else
                    {
                        query = query.Where(x => listMemberNumber.Any(y => y.Contains(x.sl.MemberNumber)) || (string.IsNullOrEmpty(x.sl.MemberNumber) && listTeam.Any(y => y == x.sl.TeamNumber)) || (listState.Any(s => s.Contains(x.cus.BusinessState)) && string.IsNullOrEmpty(x.sl.MemberNumber) && x.sl.TeamNumber == null));
                    }
                }
                else
                {
                    query = query.Where(x => listMemberNumber.Any(y => y.Contains(x.sl.MemberNumber) || (string.IsNullOrEmpty(x.sl.MemberNumber) && listTeam.Any(my => my == x.sl.TeamNumber))));
                }
                if (!string.IsNullOrEmpty(salesPerson))
                {
                    if (salesPerson == "Unassigned")
                    {
                        query = query.Where(x => string.IsNullOrEmpty(x.sl.MemberNumber));
                    }
                    else
                    {
                        query = query.Where(x => x.sl != null && x.sl.MemberNumber == salesPerson);
                    }

                }
            }
            switch (time)
            {
                //this month
                case "current-month":
                    title = "statistics by lead in this month";
                    query = query.Where(x => x.sl.CreateAt.Value.Month == currentTimeNow.Month && x.sl.CreateAt.Value.Year == currentTimeNow.Year);
                    var Lead = query.Count();
                    //Production Id: 8 = Rất quan tâm ,7= Quan tâm
                    var InterestedDemo = query.Where(x => x.sl.SL_Status == LeadSTT && (x.sl.Interaction_Status_Id == 7 || x.sl.Interaction_Status_Id == 8)).Count();
                    var Trial = query.Where(x => x.sl.SL_Status == TrialAccountSTT).Count();
                    var Merchant = query.Where(x => x.sl.SL_Status == MerchantSTT && x.cus.MxMerchant_Id != null).Count();
                    return Json(new { Lead, InterestedDemo, Trial, Merchant, title });
                //last month
                case "last-month":
                    currentTimeNow = currentTimeNow.AddMonths(-1);
                    query = query.Where(x => x.sl.CreateAt.Value.Month == currentTimeNow.Month && x.sl.CreateAt.Value.Year == currentTimeNow.Year);
                    Lead = query.Count();
                    title = "statistics by lead in last month";
                    //Production Id: 8 = Rất quan tâm ,7= Quan tâm
                    InterestedDemo = query.Where(x => x.sl.SL_Status == LeadSTT && (x.sl.Interaction_Status_Id == 7 || x.sl.Interaction_Status_Id == 8)).Count();
                    Trial = query.Where(x => x.sl.SL_Status == TrialAccountSTT).Count();
                    Merchant = query.Where(x => x.sl.SL_Status == MerchantSTT && x.cus.MxMerchant_Id != null).Count();
                    return Json(new { Lead, InterestedDemo, Trial, Merchant, title });

                case "nearest-3-months":
                    title = "statistics by lead in nearest 3 months";
                    currentTimeNow = new DateTime(currentTimeNow.Year, currentTimeNow.Month, 1).AddMonths(-2);
                    query = query.Where(x => DbFunctions.TruncateTime(x.sl.CreateAt) >= currentTimeNow);
                    Lead = query.Count();
                    //Production Id: 8 = Rất quan tâm ,7= Quan tâm
                    InterestedDemo = query.Where(x => x.sl.SL_Status == LeadSTT && (x.sl.Interaction_Status_Id == 7 || x.sl.Interaction_Status_Id == 8)).Count();
                    Trial = query.Where(x => x.sl.SL_Status == TrialAccountSTT).Count();
                    Merchant = query.Where(x => x.sl.SL_Status == MerchantSTT && x.cus.MxMerchant_Id != null).Count();
                    return Json(new { Lead, InterestedDemo, Trial, Merchant, title });

                case "current-year":
                    query = query.Where(x => x.sl.CreateAt.Value.Year == currentTimeNow.Year);
                    Lead = query.Count();
                    title = "statistics by lead in this year";
                    //Production Id: 8 = Rất quan tâm ,7= Quan tâm
                    InterestedDemo = query.Where(x => x.sl.SL_Status == LeadSTT && (x.sl.Interaction_Status_Id == 7 || x.sl.Interaction_Status_Id == 8)).Count();
                    Trial = query.Where(x => x.sl.SL_Status == TrialAccountSTT).Count();
                    Merchant = query.Where(x => x.sl.SL_Status == MerchantSTT && x.cus.MxMerchant_Id != null).Count();
                    return Json(new { Lead, InterestedDemo, Trial, Merchant, title });

                case "last-year":
                    currentTimeNow = currentTimeNow.AddYears(-1);
                    title = "statistics by lead in last year";
                    query = query.Where(x => x.sl.CreateAt.Value.Year == currentTimeNow.Year);
                    Lead = query.Count();
                    //Production Id: 8 = Rất quan tâm ,7= Quan tâm
                    InterestedDemo = query.Where(x => x.sl.SL_Status == LeadSTT && (x.sl.Interaction_Status_Id == 7 || x.sl.Interaction_Status_Id == 8)).Count();
                    Trial = query.Where(x => x.sl.SL_Status == TrialAccountSTT).Count();
                    Merchant = query.Where(x => x.sl.SL_Status == MerchantSTT && x.cus.MxMerchant_Id != null).Count();
                    return Json(new { Lead, InterestedDemo, Trial, Merchant, title });
                default:
                    throw new Exception("Time Report Incorrect");
            }
        }


        #endregion
        public ActionResult GetMemberSalesPersonByTeam(string IdTeam)
        {
            if (string.IsNullOrEmpty(IdTeam))
            {
                var sale_dep = db.P_Department.Where(d => d.Type == "SALES" && (cMem.SiteId == 1 || d.SiteId == cMem.SiteId)).Select(d => d.Id).ToList().Select(d => d.ToString());
                var ListMember = db.P_Member.Where(m => m.Active == true && sale_dep.Any(n => m.DepartmentId.Contains(n))).Select(x => new { Id = x.MemberNumber, Name = x.FullName });
                return Json(ListMember);
            }
            else
            {
                var IdDepartment = long.Parse(IdTeam);
                var Department = db.P_Department.Where(x => x.Id == IdDepartment && (cMem.SiteId == 1 || x.SiteId == cMem.SiteId)).FirstOrDefault().GroupMemberNumber.Split(',');
                var ListMember = (from a in Department join b in db.P_Member on a equals b.MemberNumber select new { Id = b.MemberNumber, Name = b.FullName }).ToList();
                return Json(ListMember);
            }
        }
        [HttpPost]
        public ActionResult Assigned(string[] SalesLeadIds, string Type, string SalesPerson)
        {
            try
            {
                SalesLeadService _salesLeadService = new SalesLeadService();
                if (Type == "Team")
                {
                    long DpId = long.Parse(SalesPerson);
                    var dp = db.P_Department.Where(x => x.Id == DpId).FirstOrDefault();
                    foreach (var SalesLeadId in SalesLeadIds)
                    {
                        var lead = db.C_SalesLead.Find(SalesLeadId);
                        if (lead != null)
                        {
                            lead.TeamNumber = DpId;
                            lead.TeamName = dp.Name;
                            db.SaveChanges();
                            _salesLeadService.CreateLog(SalesLeadId: lead.Id, SalesLeadName: lead.L_SalonName, title: "Assigned success", description: "Assigned to Team : " + dp.Name + " success", MemberNumber: cMem.MemberNumber);
                        }
                    }
                }
                else
                {
                    var mb = db.P_Member.Where(x => x.MemberNumber == SalesPerson).FirstOrDefault();
                    foreach (var SalesLeadId in SalesLeadIds)
                    {
                        var lead = db.C_SalesLead.Find(SalesLeadId);
                        if (lead != null)
                        {
                            lead.MemberNumber = SalesPerson;
                            lead.MemberName = mb.FullName;
                            db.SaveChanges();
                            _salesLeadService.CreateLog(SalesLeadId: lead.Id, SalesLeadName: lead.L_SalonName, title: "Assigned success", description: "Assigned to : " + mb.FullName + " success", MemberNumber: cMem.MemberNumber);
                        }
                    }
                }
                return Json(true);
            }
            catch (Exception ex)
            {
                WriteLogErrorService writeLogErrorService = new WriteLogErrorService();
                writeLogErrorService.InsertLogError(ex);
                return Json(false);
            }

        }
        [HttpPost]
        public ActionResult UnAssigned(string[] SalesLeadIds)
        {
            try
            {
                SalesLeadService _salesLeadService = new SalesLeadService();
                foreach (var SalesLeadId in SalesLeadIds)
                {
                    var lead = db.C_SalesLead.Find(SalesLeadId);
                    if (lead != null)
                    {
                        lead.MemberNumber = null;
                        lead.TeamNumber = null;
                        lead.MemberName = null;
                        lead.TeamName = null;
                        db.SaveChanges();
                        _salesLeadService.CreateLog(SalesLeadId: lead.Id, SalesLeadName: lead.L_SalonName, title: "UnAssigned success", description: "UnAssigned success", MemberNumber: cMem.MemberNumber);
                    }

                }

                return Json(true);
            }
            catch (Exception ex)
            {
                WriteLogErrorService writeLogErrorService = new WriteLogErrorService();
                writeLogErrorService.InsertLogError(ex);
                return Json(false);
            }

        }
        [HttpPost]
        public ActionResult Delete(string[] SalesLeadIds)
        {
            try
            {
                foreach (var SalesLeadId in SalesLeadIds)
                {
                    var lead = db.C_SalesLead.Find(SalesLeadId);
                    db.C_SalesLead.Remove(lead);
                }
                db.SaveChanges();
                return Json(true);
            }
            catch (Exception ex)
            {
                WriteLogErrorService writeLogErrorService = new WriteLogErrorService();
                writeLogErrorService.InsertLogError(ex);
                return Json(false);
            }

        }
        public ActionResult GetLogSalesLead(string SalesLeadId)
        {
            try
            {
                var code = SalesLeadId;
                bool pr_access = true;
                var currentDeparmentsUser = db.P_Department.Where(d => d.Type == "SALES" && d.GroupMemberNumber.Contains(cMem.MemberNumber));
                var ShowTeam = false;
                if ((access.Any(k => k.Key.Equals("sales_lead_assigned")) == true && access["sales_lead_assigned"] == true))
                {
                    var sale_dep = db.P_Department.Where(d => d.Type == "SALES").Select(d => d.Id).ToList().Select(d => d.ToString());
                    if (access.Any(k => k.Key.Equals("sales_lead_view_all")) == true && access["sales_lead_view_all"] == true)
                    {
                        ViewBag.ListMemberSales = db.P_Member.Where(m => m.Active == true && sale_dep.Any(n => m.DepartmentId.Contains(n))).ToList();
                        ShowTeam = true;
                    }
                    else if (access.Any(k => k.Key.Equals("sales_lead_view_team")) == true && access["sales_lead_view_team"] == true)
                    {
                        List<string> listMemberNumber = new List<string>();
                        var ManagerDep = db.P_Department.Where(x => x.SupervisorNumber == cMem.MemberNumber && x.ParentDepartmentId != null && x.Type == "SALES").ToList();
                        if (ManagerDep.Count() > 0)
                        {
                            foreach (var dep in ManagerDep)
                            {
                                listMemberNumber.AddRange(dep.GroupMemberNumber.Split(','));
                            }
                            ShowTeam = true;
                        }
                        if (currentDeparmentsUser.Count() > 0)
                        {
                            foreach (var deparment in currentDeparmentsUser)
                            {
                                if (deparment.LeaderNumber == cMem.MemberNumber)
                                {
                                    listMemberNumber.AddRange(deparment.GroupMemberNumber.Split(','));
                                }
                            }
                        }
                        listMemberNumber.Add(cMem.MemberNumber);
                        listMemberNumber = listMemberNumber.GroupBy(x => x).Select(x => x.Key).ToList();
                        ViewBag.ListMemberSales = (from m in db.P_Member join l in listMemberNumber on m.MemberNumber equals l select m).ToList();
                    }
                    ViewBag.ShowTeam = ShowTeam;
                }
                else
                {
                    var sale_dep = db.P_Department.Where(d => d.Type == "SALES").Select(d => d.Id).ToList().Select(d => d.ToString());
                    ViewBag.ListMemberSales = db.P_Member.Where(m => m.Active == true && sale_dep.Any(n => m.DepartmentId.Contains(n)) && m.MemberNumber.Contains(cMem.MemberNumber)).ToList();
                }

                IEnumerable<Calendar_Event> appointments = db.Calendar_Event.Where(a => a.SalesLeadId == code).OrderByDescending(a => a.StartEvent).ToList();

                ViewBag.pr_access = pr_access;
                var salelead = db.C_SalesLead.Find(SalesLeadId) ?? new C_SalesLead() { };
                var customer = new C_Customer()
                {
                    BusinessName = salelead.L_SalonName,
                    SalonPhone = salelead.L_Phone,
                    SalonEmail = salelead.L_Email,
                    SalonAddress1 = salelead.L_Address,
                    SalonCity = salelead.L_City,
                    SalonState = salelead.L_State,
                    BusinessZipCode = salelead.L_Zipcode,
                    BusinessCountry = salelead.L_Country,
                    ContactName = salelead.L_ContactName,
                    CellPhone = salelead.L_ContactPhone,
                    Email = salelead.L_Email,
                    More_Info = salelead.L_MoreInfo
                };
                var model = new DetailSalesLeadCustomizeModel
                {
                    even = appointments,
                    lead = getLead(code) ?? SaleLeadInfo.MakeSelect(customer, salelead, new C_SalesLead_Status() { }, true)
                };
                return PartialView("_Partial_List_Appoiment", model);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Failure! " + ex.Message;
                var model = new DetailSalesLeadCustomizeModel
                {
                    even = Enumerable.Empty<Calendar_Event>(),
                    lead = new SaleLeadInfo()
                };
                return PartialView("_Partial_List_Appoiment", model);
            }
        }
        public ActionResult LoadInteraction_Status()
        {
            var data = db.C_SalesLead_Interaction_Status.Where(x => x.Status == true).OrderBy(x => x.Order).Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name
            }).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult CreateOrUpdateStatusInteraction(C_SalesLead_Interaction_Status model)
        {
            if (model.Id > 0)
            {
                var statusInteraction = db.C_SalesLead_Interaction_Status.Find(model.Id);
                statusInteraction.Name = model.Name;
                statusInteraction.Order = model.Order;
                model.UpdatedBy = cMem.FullName;
                model.UpdatedAt = DateTime.UtcNow;
                db.SaveChanges();
                return Json(new { status = true, message = "Update success", Id = statusInteraction.Id, command = "update" });
            }
            else
            {
                model.CreatedBy = cMem.FullName;
                model.Status = true;
                model.CreatedAt = DateTime.UtcNow;
                db.C_SalesLead_Interaction_Status.Add(model);
                db.SaveChanges();
                return Json(new { status = true, message = "Create success", Id = model.Id, command = "create" });
            }


        }
        public ActionResult GetStatusAndCallOfNumber(string SalesLeadId)
        {
            var sl = db.C_SalesLead.Find(SalesLeadId);
            return Json(new { status = sl.Interaction_Status_Id, callofnumber = sl.CallOfNumber }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult GetInteractionStatusById(int? Id)
        {
            var sl = db.C_SalesLead_Interaction_Status.Find(Id);
            return Json(sl);
        }
        [HttpPost]
        public ActionResult DeleteInteractionStatus(int? Id)
        {
            var interactionstt = db.C_SalesLead_Interaction_Status.Find(Id);
            if (interactionstt != null)
            {
                db.C_SalesLead_Interaction_Status.Remove(interactionstt);
                db.SaveChanges();
            }
            return Json(new { status = true, message = "Delete success !" });
        }

        [HttpPost]
        public ActionResult LoadPopupConfirmSelectProduct(string SalesLeadId, string Command)
        {
            var sl = db.C_SalesLead.Where(c => c.Id == SalesLeadId).FirstOrDefault();
            ViewBag.ProductSelected = sl.L_License_Code;
            ViewBag.SalesLeadId = sl.Id;
            ViewBag.Command = Command;
            ReadXML xml = new ReadXML(System.Web.Hosting.HostingEnvironment.MapPath("/App_Data/Config.xml"));
            XmlNode node = xml.GetNode("/root/customer/word_determine");
            var product = new List<License_Product>();

            if (sl.L_Version == node["slice"].InnerText)
            {
                product = db.License_Product.Where(x => x.isAddon == false && x.AllowSlice == true).OrderByDescending(x => x.Price).ToList();
            }
            else
            {
                product = db.License_Product.Where(x => x.isAddon == false && x.AllowDemo == true).OrderByDescending(x => x.Price).ToList();
            }
            return PartialView("_ConfirmLicense_ProductPopup", product);
        }
        [HttpPost]
        public ActionResult ProcessConfirmSelectProduct(string SalesLeadId, string LicenseId)
        {
            try
            {
                var sl = db.C_SalesLead.Where(c => c.Id == SalesLeadId).FirstOrDefault();
                var product = db.License_Product.Find(LicenseId);
                sl.L_License_Code = product.Code;
                sl.L_License_Name = product.Name;
                db.SaveChanges();
                return Json(new { status = true });
            }
            catch (Exception ex)
            {
                WriteLogErrorService writeLogErrorService = new WriteLogErrorService();
                writeLogErrorService.InsertLogError(ex);
                return Json(new { status = false });
            }
        }

        [HttpGet]
        public async Task<ActionResult> SyncingSalesLeadFromMonday()
        {
            try
            {
				var _goHighLevelService = EngineContext.Current.Resolve<IGoHighLevelConnectorService>();
				 await _goHighLevelService.SyncingSalesLeadFromGoHighLevelAsync();
				return Json(new { status = true, message = "Sync saleslead from gohighlevel successfully" }, JsonRequestBehavior.AllowGet);
			
			}
            catch(Exception ex)
            {
				return Json(new { status = false, message = "Sync saleslead gohighlevel failed" }, JsonRequestBehavior.AllowGet);
			}
          
        }
    }
}
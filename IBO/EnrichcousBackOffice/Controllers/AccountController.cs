using Enrich.IServices;
using Enrich.IServices.Utils.Mailing;
using Enrich.IServices.Utils.OAuth;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Models.CustomizeModel;
using EnrichcousBackOffice.Utils.IEnums;
using EnrichcousBackOffice.ViewControler;
using Inner.Libs.Helpful;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Newtonsoft.Json;
using PdfTextCoordinate;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;


namespace EnrichcousBackOffice.Controllers
{
    [Authorize]
    public class AccountController : UploadController
    {
        WebDataModel db = new WebDataModel();
        private readonly ILogService _logService;
        private readonly IMailingService _mailingService;
        private readonly IEnrichOAuth _enrichOAuth;

        public AccountController(
            ILogService logService,
            IMailingService mailingService,
            IEnrichOAuth enrichOAuth)
        {
            _logService = logService;
            _mailingService = mailingService;
            _enrichOAuth = enrichOAuth;
        }

        // GET: Account
        public ActionResult Index()
        {
            Authority.GetAccessAuthority(true);
            return View();
        }

        #region login-logout-forgotpass


        [AllowAnonymous]
        public ActionResult Login()
        {

            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> LoginSubmit()
        {
            try
            {
                WebDataModel db = new WebDataModel();
                var email = Request["email"];
                var password = Request["password"];
                var remember_me = Request["remember-me"] != null ? true : false;

                var member = db.P_Member.Where(m => m.PersonalEmail.ToLower().Equals(email.ToLower()) && m.Password.Equals(password) && m.Delete != true).FirstOrDefault();
                var accessToken =  _enrichOAuth.GetAccessToken(email, password);
                if (member == null || string.IsNullOrEmpty(accessToken))
                {
                    _logService.Info($"[Account][Login] Login failed email: {email} at {DateTime.Now}", new
                    {
                        contextId = Guid.NewGuid(),
                        content = $"Login failed email: {email} at {DateTime.UtcNow} - Email or password is incorrect",

                        //request = JsonConvert.SerializeObject(Request)
                    });
                    throw new Exception("Login failed! Email or password is incorrect.");
                }

                if (member.Active != true)
                {
                    _logService.Info($"[Account][Login] Login failed email: {email} at {DateTime.Now}", new
                    {
                        contextId = Guid.NewGuid(),
                        content = $"Login failed email: {email} at {DateTime.UtcNow} Account has been deactivated",

                    });
                    throw new Exception("Login failed! account has been deactivated");
                }

                Session["member"] = member;
                Session["ACCESS_TOKEN"] = accessToken;

                string traceTrackId = Guid.NewGuid().ToString();
                Session["traceTrackId"] = traceTrackId;
                FormsAuthentication.SetAuthCookie(member.PersonalEmail, remember_me);
                
                Authority.GetAccessAuthority(true);
                _logService.Info($"[Account][Login] Login success email: {email} at {DateTime.Now}", new
                {
                    TraceTrackId = traceTrackId,
                    content = $"Login success email: {email} at {DateTime.Now}",
                });

                if (string.IsNullOrEmpty(Request["ReturnURL"]) || !Url.IsLocalUrl(Request["ReturnURL"]))
                {
                    //is sales person
                    if (!string.IsNullOrEmpty(member.DepartmentId) && member.DepartmentId.Contains("19120010"))
                    {
                        return Redirect("/SaleLead");
                    }
                    else
                    {
                        return Redirect("/Ticket_New");
                    }
                }
                var referer = Request.Headers["Referer"];
                if (referer.Contains("account/login")) return Redirect(Request["ReturnURL"]);
                return Redirect(referer);
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return RedirectToAction("login");
            }
        }

        public ActionResult Logout(string id)
        {
            if (id == "email_change")
            {
                TempData["error"] = "Your email persion has just been changed. Please log in again.";
            }
            if (id == "deactive")
            {
                TempData["error"] = "Your account has been deactive. Please log in again.";
            }
            FormsAuthentication.SignOut();
            Session.RemoveAll();

            return RedirectToAction("login");
        }

        /// <summary>
        /// gui email mat khau
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        public async Task<JsonResult> CheckForgotPass(string email)
        {
            using (var db = new WebDataModel())
            {
                try
                {
                    //check email ton tai
                    var member = db.P_Member.Where(m => m.PersonalEmail.Equals(email)).FirstOrDefault();
                    if (member != null)
                    {
                        if (member.Active == true)
                        {
                            string bodyM = "<p>Dear " + member.FirstName + ", </p> " +
                                           "<p>You are receiving this notification because you forgot your password.</p>" +
                                           "<p>Your account information is as follows:</p>" +
                                           "---------------------------- " +
                                           "<p>Username: <b>" + member.PersonalEmail + "</b></p>" +
                                           "<p>password : <b>" + member.Password + "</b> </p> " +
                                           "----------------------------<br/><br/>" +

                                          "Best Regards,<br/> Support Team  <br/>";

                            //send email
                            string resEmail = await _mailingService.SendBySendGrid(email, member.FirstName, "[Enrichcous]Forgot your password", bodyM);
                            if (string.IsNullOrWhiteSpace(resEmail))
                            {
                                return Json(new object[] { true, "Email has been sent, Please check your mail box. If not found, try searching in the SPAM folder." });
                            }
                            else
                            {
                                throw new Exception("OOPs, We had problems sending, Please check your email and try again later!");
                            }
                        }
                        else
                        {
                            throw new Exception("Your account is inactivated. Please contact your manager to supported");
                        }
                    }
                    else
                    {
                        throw new Exception("Email not found.");
                    }
                }
                catch (Exception e)
                {
                    return Json(new object[] { false, "Can't send it!. " + e.Message });
                }
            }
        }

        #endregion

        #region register

        [AllowAnonymous]
        public ActionResult Register()
        {

            WebDataModel db = new WebDataModel();
            ViewBag.MemType = db.P_MemberType.OrderBy(c => c.Name).ToList();
            var ct = from c in db.Ad_Country
                     select new
                     {
                         Country = c.Name,
                         Name = c.Name
                     };
            ViewBag.Countries = new SelectList(ct, "Country", "Name", "United States");
            var um = new P_MemberSubscription();
            if (Session["member_subcriber"] != null)
            {
                um = Session["member_subcriber"] as P_MemberSubscription;
            }
            return View(um);
        }


        [AllowAnonymous]
        public JsonResult GetNameRefered(string ReferNumber)
        {
            var mb = (from mem in db.P_Member
                      where mem.MemberType == "distributor" && mem.MemberNumber == ReferNumber
                      select mem).FirstOrDefault();
            var name = mb?.FullName;
            return Json(name ?? "");
        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> SaveRegister(P_MemberSubscription MemSub)
        {

            try
            {
                var checkExistEmail = db.P_Member.Any(m => (m.PersonalEmail.Equals(MemSub.PersonalEmail, StringComparison.InvariantCultureIgnoreCase) == true
                || m.CellPhone.Equals(MemSub.CellPhone, StringComparison.InvariantCultureIgnoreCase) == true) && m.Delete != true);
                if (checkExistEmail)
                {
                    throw new Exception("Your information already exists, Please recheck your email and phone number or contact with us tobe supported");
                }

                string memberTypeName = db.P_MemberType.Where(t => t.MemberType.Equals(MemSub.MemberType)).FirstOrDefault()?.Name;
                MemSub.MemberTypeName = memberTypeName;
                MemSub.CreateAt = DateTime.UtcNow;

                MemSub.Id = long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssff"));
                MemSub.SocialSecurity = string.IsNullOrWhiteSpace(MemSub.SocialSecurity) == true ? "" : MemSub.SocialSecurity.Replace(" ", "");
                if (!string.IsNullOrWhiteSpace(MemSub.SocialSecurity) && MemSub.SocialSecurity.Length >= 9)
                {
                    var s1 = MemSub.SocialSecurity.Substring(0, 3);
                    var s2 = MemSub.SocialSecurity.Substring(3, 2);
                    var s3 = MemSub.SocialSecurity.Substring(5, 4);

                    if (MemSub.SocialSecurity.Split(new char[] { '-' }).Length == 3)
                    {
                        s1 = MemSub.SocialSecurity.Split(new char[] { '-' })[0];
                        s2 = MemSub.SocialSecurity.Split(new char[] { '-' })[1];
                        s3 = MemSub.SocialSecurity.Split(new char[] { '-' })[2];
                    }
                    else if (MemSub.SocialSecurity.Split(new char[] { '.' }).Length == 3)
                    {
                        s1 = MemSub.SocialSecurity.Split(new char[] { '.' })[0];
                        s2 = MemSub.SocialSecurity.Split(new char[] { '.' })[1];
                        s3 = MemSub.SocialSecurity.Split(new char[] { '.' })[2];
                    }

                    MemSub.SocialSecurity = string.Join("-", s1, s2, s3);
                }
                db.P_MemberSubscription.Add(MemSub);
                db.SaveChanges();
                TempData["s"] = "Thank you for subcription, Your account has been successfully registered and is pending for approval. Once your account is approved, you will be receiving a notification email with your account login.";

                string link = Request.Url.Scheme + "://" + Request.Url.Authority + "/memberman/subcription/";
                string bodyMailReferral = @"A new partner has just registered on enrich backoffice website!<br/>" + @"<br/>
                                ::BELOW IS HIS/HER INFOMATION::<br/>
                                <b class='w120'>Name</b>:" + MemSub.FirstName + " " + MemSub.LastName + @"<br/>
                                <b  class='w120'>Address</b>:" + MemSub.Address + @"<br/>
                                <b  class='w120'>Cellphone</b>:" + MemSub.CellPhone + @"<br/>
                                <b  class='w120'>Email</b>:" + MemSub.PersonalEmail + @"<br/>
                                <b  class='w120'>Member type</b>:" + MemSub.MemberTypeName + @"<br/>
                                <b  class='w120'>Referal by</b>:" + MemSub.ReferedByName + @"<br/>
                                <b  class='w120'>Register at</b>:" + DateTime.Now.ToString("MM/dd/yyyy hh:mm tt") + @"<br/>
                                <br/>
Please validate this person by clicking on the link below.<br/>
                                <br/><a href='" + link + @"'>" + link + @"</a>
                                <br/>";
                var info = db.SystemConfigurations.FirstOrDefault();
                await _mailingService.SendBySendGrid(info.HREmail, "", "[Enrich]A new member has just registered", bodyMailReferral, info.NotificationEmail);
                Session.Remove("member_subcriber");
                return RedirectToAction("Register");
            }
            catch (Exception e)
            {
                Session["member_subcriber"] = MemSub;
                TempData["e"] = "Oops!." + e.Message;
            }
            return RedirectToAction("Register");
        }

        #endregion

        #region My profile

        public ActionResult MyProfile()
        {
            try
            {
                var curMem = Authority.GetCurrentMember(true);

                if (curMem == null)
                {
                    return RedirectToAction("login");
                }

                WebDataModel db = new WebDataModel();
                var ct = from c in db.Ad_Country
                         select new
                         {
                             Country = c.Name,
                             Name = c.Name
                         };

            
                ViewBag.MoreFiles = db.UploadMoreFiles.Where(f => f.TableId == curMem.Id && f.TableName.Equals("P_Member")).ToList();

                ViewBag.Countries = new SelectList(ct, "Country", "Name", curMem.Country ?? "United States");
                ViewBag.CurLevel = db.P_MemberLevel.Where(l => l.MemberNumber == curMem.MemberNumber).FirstOrDefault();

                //so luong nhan vien duoc curmem moi vao
                ViewBag.EmployesNumber = db.P_Member.Where(m => m.ReferedByNumber == curMem.MemberNumber).ToList().Count();
                return View(curMem);
            }
            catch (Exception ex)
            {
                TempData["error"] = "Error: " + ex.Message;
                return RedirectToAction("login");
            }
        }


        /// <summary>
        /// update my profile
        /// </summary>
        /// <param name="member">object P_Member</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> UpdateMyProfile(P_Member member)
        {
            try
            {
                WebDataModel db = new WebDataModel();
                P_Member curMem = db.P_Member.Find(member.Id);

                string deleteFileFlag = Request["hdPicDelete_pic0"];
                UploadAttachFile("/upload/employees", "pic0", "", "employees_" + curMem.MemberNumber.ToString() + ".png", out string picture);
                bool login_email_change = false;
                if (deleteFileFlag == "1" || string.IsNullOrWhiteSpace(picture) == false)
                {
                    curMem.Picture = picture;
                }
                if (db.P_Member.Any(m => m.PersonalEmail == member.PersonalEmail && m.Id != member.Id))
                {
                    throw new Exception("Email already using by other member");
                }
                if (curMem.PersonalEmail != member.PersonalEmail)
                {
                    curMem.PersonalEmail = member.PersonalEmail;
                    login_email_change = true;
                }
                curMem.FirstName = member.FirstName;
                curMem.LastName = member.LastName;
                curMem.FullName = member.FirstName + " " + member.LastName;
                curMem.Country = member.Country;
                curMem.State = member.State;
                curMem.City = member.City;
                curMem.ZipCode = member.ZipCode;
                curMem.Address = member.Address;
                curMem.CellPhone = member.CellPhone;
                curMem.Gender = member.Gender;
                curMem.SocialInsuranceCode = member.SocialInsuranceCode;
                curMem.PersonalIncomeTax = member.PersonalIncomeTax;
                curMem.IdentityCardNumber = member.IdentityCardNumber;
                curMem.TimeZone = member.TimeZone;
                curMem.GMTNumber = Ext.EnumParse<TIMEZONE_NUMBER>(member.TimeZone).Code<int>();
                curMem.ProfileDefinedColor = member.ProfileDefinedColor;
                DateTime.TryParse(Request["Birthday"], out DateTime Birthday);
                if (Birthday.Year > 1900)
                {
                    curMem.Birthday = Birthday;
                }

                curMem.UpdateBy = curMem.FullName.ToUpper();
                curMem.UpdateAt = DateTime.UtcNow;

                //save file attachment
                int filescount = Request.Files.Count;
                await UploadMultipleFilesAsync(member.Id, "P_Member", filescount, "/upload/employees");
                //.end

                db.Entry(curMem).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Json(new object[] { true, "Update success.", login_email_change });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, "Fail: " + e.Message });
            }
        }


        /// <summary>
        /// doi mat khau
        /// </summary>
        /// <param name="p1">pass hien co</param>
        /// <param name="p2">pass moi</param>
        /// <param name="p3">nhap lai pass moi</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> ChangePass(string p1, string p2, string p3)
        {
            try
            {
                WebDataModel db = new WebDataModel();
                var curMem = Authority.GetCurrentMember();

                if (curMem.Password.Equals(p1))
                {
                    if (p2.Equals(p3))
                    {
                        //ok. doi pass
                        var member = db.P_Member.Find(curMem.Id);
                        if (member.Active == true)
                        {
                            member.Password = p2;
                            db.Entry(member).State = System.Data.Entity.EntityState.Modified;

                            //send email after changed password
                            await _mailingService.SendEmailAfterChangedPass(member.FirstName, member.PersonalEmail, member.Password, member.PersonalEmail);
                            curMem.Password = p2;
                        }
                        else
                        {
                            throw new Exception("Your account is inactivated. Please contact your manager to supported");
                        }
                    }
                    else
                    {
                        throw new Exception("Password incorrect, please try again!");
                    }
                }
                else
                {
                    throw new Exception("Current password is not correct");
                }

                db.SaveChanges();
                return Json(new object[] { true, "Change password successfully!" });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, "Error: " + e.Message });
            }
        }

        #endregion

        #region Hierachy Employee

        public ActionResult MyTeam()
        {
            return View();
        }


        private void SetChildrentEmployee(HierarchyEmployeesViewModel preEmp, IQueryable<HierarchyEmployeesViewModel> hieEmp)
        {
            var childEmp = from emp in hieEmp
                           where emp.ReferedByMemberNumber == preEmp.MemberNumber
                           select emp;

            if (childEmp != null && childEmp.Count() > 0)
            {
                foreach (var item in childEmp)
                {
                    if (hieEmp.Where(e => e.ReferedByMemberNumber == item.MemberNumber).Count() == 0)
                    {
                        if (item.Active == false)
                        {
                            continue;
                        }

                        preEmp.ChildEmployees.Add(item);
                        continue;
                    }
                    SetChildrentEmployee(item, hieEmp);
                    preEmp.ChildEmployees.Add(item);
                }
            }
        }

        #endregion

        #region Get Employees Number

        public JsonResult GetEmpNumber()
        {
            try
            {
                var cMem = Authority.GetCurrentMember();
                var empNumber = 0;

                var department = db.P_Department.Where(d => d.LeaderNumber.Contains(cMem.MemberNumber)).FirstOrDefault();
                if (department != null)
                {
                    //cMem la department director
                    string idDept = department.Id.ToString();
                    empNumber = db.P_Member.Where(m => m.DepartmentId.Contains(idDept) == true && m.MemberNumber != cMem.MemberNumber && m.Active == true).ToList().Count();
                }
                else
                {
                    //cMem k phai department director
                    empNumber = EmployeesViewService.GetMemberNumberInBranch(cMem.MemberNumber).Count() - 1;
                }

                return Json(new object[] { true, empNumber }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        /// <summary>
        /// Get thong tin co ban thanh vien, thuong duoc su dung trong popup view member infomation
        /// </summary>
        /// <param name="mn"></param>
        /// <returns>_MemberProfileSimplePartial</returns>
        public ActionResult GetMemberInfoSimple(string mn)
        {
            using (var db = new WebDataModel())
            {
                var member = db.P_Member.Where(m => m.MemberNumber == mn).FirstOrDefault() ?? new P_Member() { };
                ViewBag.BelongToPartners = string.Join(", ", db.C_Partner.Where(c => member.BelongToPartner.Contains(c.Code)).Select(c => "#" + c.Code + " - " + c.Name).ToList());
                return PartialView("_MemberProfileSimplePartial", member);
            }
        }



        /// <summary>
        /// Get USA State
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public JsonResult GetUSAStates()
        {
            WebDataModel db = new WebDataModel();
            var result = from s in db.Ad_USAState
                         orderby s.abbreviation
                         select new
                         {
                             ID = s.abbreviation,
                             Name = s.name
                         };

            return Json(result.ToList());
        }

        //[HttpGet]
        //public ActionResult ProcessFilesPayrollPDF()
        //{
        //    PdfReader reader
        //      = new PdfReader("D:\\123.pdf");
        //    PdfDocument pdf = new PdfDocument(reader);
        //    Rectangle rect = new Rectangle(300, 700, 100, 30); ;
        //    TextRegionEventFilter regionFilter = new TextRegionEventFilter(rect);
        //    StringBuilder builder = new StringBuilder();

        //    for (int page = 1; page <= pdf.GetNumberOfPages(); page++)
        //    {
        //        ITextExtractionStrategy strat = new FilteredTextEventListener(new LocationTextExtractionStrategy(), regionFilter);
        //        string str = PdfTextExtractor.GetTextFromPage(pdf.GetPage(page), strat) + "\n\n";
        //        builder.Append(str);
        //    }
        //    return Content(builder.ToString());


        //}

        [HttpGet]
        public ActionResult ProcessFilesPayrollPDF()
        {
            using (var reader = new PdfReader("D:\\123.pdf"))
            {

                var parser = new PdfReaderContentParser(reader);

                var strategy = parser.ProcessContent(1, new LocationTextExtractionStrategyWithPosition());

                var res = strategy.GetLocations();

                reader.Close();
                var searchResult = res.Where(p => p.Text.Contains("ENR")).OrderBy(p => p.Y).Reverse().ToList();
                return Content(searchResult.FirstOrDefault(x => x.Text.Length == 9).Text);
            }
        }
    }
}
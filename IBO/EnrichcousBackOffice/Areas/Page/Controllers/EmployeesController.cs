using Enrich.Core.Ultils;
using Enrich.IServices.Utils.Mailing;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace EnrichcousBackOffice.Areas.Page.Controllers
{
    [Authorize]
    public class EmployeesController : Controller
    {
        private readonly IMailingService _mailingService;

        public EmployeesController(IMailingService mailingService)
        {
            _mailingService = mailingService;
        }

        // GET: Page/Employee
        public ActionResult Index(string key)
        {
            WebDataModel db = new WebDataModel();
            var MemberNumber = SecurityLibrary.Decrypt(key);
            var member = db.P_Member.Where(x => x.MemberNumber == MemberNumber).FirstOrDefault();
            if (member == null)
            {
                return Redirect("/");
            }
            if (string.IsNullOrEmpty(member.Country))
            {
                member.Country = "US";
            }
            var ct = from c in db.Ad_Country.Where(x => x.CountryCode == "US" || x.CountryCode == "VN")
                     select new
                     {
                         Country = c.CountryCode,
                         Name = c.Name
                     };
            var provinces = db.Ad_Provinces.Where(x => x.CountryId == member.Country).ToList();
            // ViewBag.MoreFiles = db.UploadMoreFiles.Where(f => f.TableId == member.Id && f.TableName.Equals("P_Member")).ToList();
            ViewBag.Provinces = new SelectList(provinces, "Code", "Name");
            ViewBag.Countries = new SelectList(ct, "Country", "Name");
            var card  = db.P_EmployeeBankCard.Where(x => x.MemberNumber == MemberNumber).OrderBy(x=>x.IsDefault==true).FirstOrDefault();
            if (card != null)
            {
                string default_pass = System.Configuration.ConfigurationManager.AppSettings["PassCode"];
                card.CardNumber = SecurityLibrary.Decrypt(card.CardNumber, default_pass + card.CreatedAt.Date.Ticks);
            }
            ViewBag.EmployeesBank = card;
            return View(member);
        }
        public ActionResult Complete(string key)
        {
            WebDataModel db = new WebDataModel();
            var MemberNumber = SecurityLibrary.Decrypt(key);
            var member = db.P_Member.Where(x => x.MemberNumber == MemberNumber).FirstOrDefault();
            if (member == null)
            {
                return Redirect("/");
            }
            if (member.IsCompletedUpdateInfo != true)
            {
                return RedirectToAction("Index", new { key = key });
            }
            if (!string.IsNullOrEmpty(member.Country))
            {
                ViewBag.Country = db.Ad_Country.FirstOrDefault(x => x.CountryCode == member.Country)?.Name;
            }
            if (!string.IsNullOrEmpty(member.State))
            {
                ViewBag.State = db.Ad_Provinces.FirstOrDefault(x => x.Code == member.State)?.Name;
            }
            ViewBag.Key = key;
            return View(member);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveInfomation(P_Member model)
        {
            P_Member cMem = AppLB.Authority.GetCurrentMember();
            WebDataModel db = new WebDataModel();
            var member = db.P_Member.Where(x => x.Id == model.Id).FirstOrDefault();
            member.FirstName = model.FirstName;
            member.LastName = model.LastName;
            member.CellPhone = model.CellPhone;
            member.Country = model.Country;
            member.State = model.State;
            member.City = model.City;
            member.Email1 = model.Email1;
            member.SocialInsuranceCode = model.SocialInsuranceCode;
            member.PersonalIncomeTax = model.PersonalIncomeTax;
            member.JobPosition = model.JobPosition;
            member.Address = model.Address;
            member.Birthday = model.Birthday;
            member.FullName = member.FirstName + " " + member.LastName;
            member.ZipCode = model.ZipCode;
            member.Gender = model.Gender;
            member.UpdateAt = DateTime.UtcNow;
            member.UpdateBy = cMem.FullName;
            member.IdentityCardNumber = model.IdentityCardNumber;
            //avatar
            string Avatar_Path = "/upload/employees/" + member.MemberNumber + "/avatar";
            string Avatar_AbsolutePath = Server.MapPath(Avatar_Path);
            if (!Directory.Exists(Avatar_AbsolutePath))
                Directory.CreateDirectory(Avatar_AbsolutePath);

            if (Request.Files["avatar_upload"] != null)
            {
                var avatar = Request.Files["avatar_upload"];
                if (avatar.ContentLength > 0)
                {
                    avatar.SaveAs(Avatar_AbsolutePath + "/" + avatar.FileName);
                    member.Picture = Avatar_Path + "/" + avatar.FileName;
                }
            }

            //identity image
            string IdentityCard_Path = "/upload/employees/" + member.MemberNumber + "/identitycard";
            string IdentityCard_AbsolutePath = Server.MapPath(IdentityCard_Path);
            if (!Directory.Exists(IdentityCard_AbsolutePath))
                Directory.CreateDirectory(IdentityCard_AbsolutePath);

            if (Request.Files["IdentityCardImageBefore"] != null)
            {
                var IdentityCardImageBefore = Request.Files["IdentityCardImageBefore"];
                if (IdentityCardImageBefore.ContentLength > 0)
                {
                    IdentityCardImageBefore.SaveAs(IdentityCard_AbsolutePath + "/" + IdentityCardImageBefore.FileName);
                    member.IdentityCardImageBefore = IdentityCard_Path + "/" + IdentityCardImageBefore.FileName;
                }
            }


            if (Request.Files["IdentityCardImageAfter"] != null)
            {
                var IdentityCardImageAfter = Request.Files["IdentityCardImageAfter"];
                if (IdentityCardImageAfter.ContentLength > 0)
                {
                    IdentityCardImageAfter.SaveAs(IdentityCard_AbsolutePath + "/" + IdentityCardImageAfter.FileName);
                    member.IdentityCardImageAfter = IdentityCard_Path + "/" + IdentityCardImageAfter.FileName;
                }
            }
          
            if (member.IsCompletedUpdateInfo != true)
            {
                var emailEncrypt = SecurityLibrary.Encrypt(member.PersonalEmail);
                var url = WebConfigurationManager.AppSettings["IMSUrl"] + "/Page/employees/complete?key=" + emailEncrypt;
                var humanResourceEmail = db.SystemConfigurations.FirstOrDefault();
                if (!string.IsNullOrEmpty(humanResourceEmail.HREmail))
                {
                    string subject = $"Enrich: Update Information Completed";
                    string content = $"Hello,<br/>" +
                                     $"<p><b>{member.FullName}</b> just completed information !</p>" +
                                     $"<a style='background: #00c0ef; color: white; padding: 4px 18px; margin-top: 11px; display: inline-block; text-decoration: none; border-radius: 3px;' href=" + url + " style='margin-left: 25px;'><b>View Detail</b></a><br/><br/>" +
                                     $"<p>Sincerely,</p>";
                    var emailData = new { content = content, subject = subject };
                  
                    var msg = await _mailingService.SendNotifyOutgoingWithTemplate(humanResourceEmail.HREmail, "", subject, "", emailData);
                }
                
            }
            member.IsCompletedUpdateInfo = true;


            // card 
            var card = db.P_EmployeeBankCard.Where(x => x.MemberNumber == member.MemberNumber).OrderBy(x => x.IsDefault == true).FirstOrDefault();
            string default_pass = System.Configuration.ConfigurationManager.AppSettings["PassCode"];
            if (card != null)
            {
               
                card.BankName = Request["BankName"];
                card.BranchNameBank = Request["BranchNameBank"];
                card.CardNumber = SecurityLibrary.Encrypt(Request["CardNumber"], default_pass + card.CreatedAt.Date.Ticks);
                card.UpdateAt = DateTime.UtcNow;
                card.UpdateBy = cMem.FullName;
            }
            else
            {
                var newCard = new P_EmployeeBankCard();
                newCard.CreatedAt = DateTime.UtcNow;
                newCard.CreatedBy = cMem.FullName;
                newCard.BankName = Request["BankName"];
                newCard.BranchNameBank = Request["BranchNameBank"];
                newCard.CardNumber = SecurityLibrary.Encrypt(Request["CardNumber"], default_pass + newCard.CreatedAt.Date.Ticks);
                newCard.MemberNumber = member.MemberNumber;
                db.P_EmployeeBankCard.Add(newCard);
                db.SaveChanges();
            }
            var key = SecurityLibrary.Encrypt(member.MemberNumber);
            db.SaveChanges();
            return RedirectToAction("Complete", new { key = key });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Complete(P_Member model, string key)
        {
            P_Member cMem = AppLB.Authority.GetCurrentMember();
            WebDataModel db = new WebDataModel();
            var member = db.P_Member.Where(x => x.Id == model.Id).FirstOrDefault();
            if (member == null)
            {
                return Redirect("/");
            }
            if (member.IsCompletedUpdateInfo != true)
            {
                return RedirectToAction("Index", new { key = key });
            }
            member.IsCompletedUpdateInfo = true;
            db.SaveChanges();
            return RedirectToAction("Thanks", new { key = key });
        }
        public ActionResult Thanks(string key)
        {

            WebDataModel db = new WebDataModel();
            var MemberNumber = SecurityLibrary.Decrypt(key);
            var member = db.P_Member.Where(x => x.MemberNumber == MemberNumber).FirstOrDefault();
            if (member == null)
            {
                return Redirect("/");
            }
            if (member.IsCompletedUpdateInfo != true)
            {
                return RedirectToAction("Index", new { key = key });
            }

            return View();
        }
    }
}
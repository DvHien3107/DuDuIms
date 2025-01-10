using Enrich.IServices;
using Promotion.Mango.Models;
using Promotion.Mango.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace Promotion.Mango.Controllers
{
    public class HomeController : Controller
    {
       private readonly ILogService _logService;


        public HomeController(ILogService logService)
        {
            _logService = logService;
        }
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Contact(ContactModel contact)
        {
            try
            {
                var lead = new C_SalesLead();
                lead.Id = DateTime.UtcNow.ToString("yyMMddhhmmssfff");
                lead.L_ContactPhone = contact.Phone;
                lead.L_Phone = contact.Phone; 
                lead.L_ContactName = contact.FullName;
                lead.L_Email = contact.Email;
                lead.L_ContactName = contact.FullName;
                
                lead.L_SalonName = contact.SalonName;
                lead.LastNote = contact.Message;
                lead.LastNoteAt = DateTime.UtcNow;
                lead.CreateBy = "Enrich Promotion";
                lead.CreateAt = DateTime.UtcNow;
                using (var db = new EnrichcousBOEntities())
                {
                    db.C_SalesLead.Add(lead);
                    db.SaveChanges();

                }
                var service = new SendEmailService();
                string ToEmail = WebConfigurationManager.AppSettings["MailTo"];
                string cc = WebConfigurationManager.AppSettings["Mailcc"];
                var SalesLeadUrl = WebConfigurationManager.AppSettings["IMSUrl"]+"/salelead";
                string contentAdmin = $"Hello,<br/><br/>" +
                             $"<p>You have new subscribe from Enrich Promotion !</p></br></br>" +
                             $"<p>Full Name: {contact.FullName}</p></br>" +
                             $"<p>Phone Number: {contact.Phone}</p></br>" +
                             $"<p>Email: {contact.Email}</p></br>" +
                             $"<p>Salon Name: {contact.SalonName}</p></br>" +
                             $"<p>Message: {contact.Message}</p></br>" +
                             $"<a style='background: #00c0ef; color: white; padding: 4px 18px; margin-top: 11px; display: inline-block; text-decoration: none; border-radius: 3px;' href=" + SalesLeadUrl + " style='margin-left: 25px;'><b>View Detail</b></a><br/><br/>" +
                             $"<p>Sincerely,";
                var emailData = new { content = contentAdmin, subject = "New Subscribe From Enrich Promotion" };
                //send email to admin
                await service.SendEmailContact(ToEmail, "", "Subscribe success Enrich Promotion", cc, emailData);
                //send email to member subscribe
                string contentCustomer = $"Hello {contact.FullName},<br/><br/>" +
                           $"<p>Thank you for connecting with us.  One of our sales consultant will reach out to you soon. </p></br>" +
                           $"<p>Sincerely,</p></br></br>";
                var emailDataTosubscriber = new { content = contentCustomer, subject = "Subscribe success Mango Enrich" };
                await service.SendEmailContact(contact.Email, contact.FullName, "Subscribe success Enrich Promotion", "", emailDataTosubscriber);
                _logService.Info("subscribe success");
                return Json(new { status = true, message = "subscribe success" });
            }
            catch(Exception ex)
            {
                _logService.Error("subscribe failed", ex);
                return Json(new { status = false, message = "subscribe failed" });
            }
        }
    }
}
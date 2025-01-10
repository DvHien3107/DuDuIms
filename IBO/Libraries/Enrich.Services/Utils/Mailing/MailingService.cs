using Enrich.Core.Ultils;
using Enrich.Core.UnitOfWork;
using Enrich.Core.UnitOfWork.Data;
using Enrich.DataTransfer;
using Enrich.Entities;
using Enrich.IServices.Utils.Mailing;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace Enrich.Services.Utils.Mailing
{
    public class MailingService : IMailingService
    {
        private readonly string _apikeySG;
        private readonly string smtp;
        private readonly string webmail;
        private readonly string webmail_displayname;
        private readonly string webmailpass;
        private readonly int webmailport;
        private readonly bool ssl;
        private readonly string _url;
        private readonly string _domain;
        private readonly string _replyto;
        private readonly string _replyname;
        private readonly SystemConfiguration sysConfig;
        private readonly EnrichContext _enrichContext;
        private readonly IUnitOfWork _unitOfWork;

        public MailingService(IUnitOfWork unitOfWork, EnrichContext enrichContext)
        {
            _unitOfWork = unitOfWork;
            _enrichContext = enrichContext;
            _apikeySG = ConfigurationManager.AppSettings["SendGridApiKey"];
            smtp = ConfigurationManager.AppSettings["SMTP"];
            webmail = ConfigurationManager.AppSettings["WebMail"];
            webmail_displayname = ConfigurationManager.AppSettings["WebMailDisplayName"];
            webmailpass = ConfigurationManager.AppSettings["WebMailPass"];
            webmailport = int.Parse(ConfigurationManager.AppSettings["WebMailPort"]);
            ssl = bool.Parse(ConfigurationManager.AppSettings["WebMailSSL"]);
            _domain = ConfigurationManager.AppSettings["Domain"];
            _url = ConfigurationManager.AppSettings["IMSUrl"];
            _replyto = ConfigurationManager.AppSettings["ReplyEmail"];
            _replyname = ConfigurationManager.AppSettings["ReplyName"];
            sysConfig = _unitOfWork.Repository<SystemConfiguration>().TableNoTracking.FirstOrDefault();
        }

        public async Task<string> Send(string to, string subject, string body, string bcc = "", string cc = "", string fileAttach = null)
        {
            try
            {
                if (!_enrichContext.IsProduction) to = ConfigurationManager.AppSettings["WebMail"];
                SmtpClient client = new SmtpClient(smtp);
                client.UseDefaultCredentials = false;
                NetworkCredential nc = new NetworkCredential(webmail, webmailpass);
                client.Credentials = nc;
                client.Port = webmailport;
                client.EnableSsl = ssl;

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(webmail, webmail_displayname);
                mail.Sender = new MailAddress(webmail, webmail_displayname);
                string[] arrTo = to.Split(new char[] { ';' });
                for (int i = 0; i < arrTo.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(arrTo[i]) == false && mail.To.Any(e => e.Address.Equals(arrTo[i])) == false)
                    {
                        mail.To.Add(new MailAddress(arrTo[i]));
                    }
                }

                if (string.IsNullOrWhiteSpace(bcc) == false)
                {
                    //bcc
                    if (string.IsNullOrWhiteSpace(bcc) == false)
                    {
                        string[] ademail = bcc.Split(new char[] { ';' });
                        foreach (var item in ademail)
                        {
                            if (string.IsNullOrWhiteSpace(item) == false && to.Contains(item) == false)
                            {
                                mail.Bcc.Add(new MailAddress(item));
                            }
                        }
                    }
                }
                if (string.IsNullOrWhiteSpace(cc) == false)
                {
                    //cc
                    string[] ccArr = cc.Split(new char[] { ';' });
                    foreach (var item in ccArr)
                    {
                        if (string.IsNullOrWhiteSpace(item) == false && to.Contains(item) == false)
                        {
                            mail.CC.Add(new MailAddress(item));
                        }
                    }
                }

                SystemConfiguration webinfo = sysConfig;
                mail.Subject = subject;
                mail.Body = @"<!DOCTYPE><html xmlns=""http://www.w3.org/1999/xhtml""><head>
                            <style>
                                .table_border, .table_border th, .table_border  td {
                                    border: 1px solid grey;
                                }
                                .w150{
                                    font-weight:bold;
                                    display:inline-block;
                                    width:150px;
                                    padding-right:3px
                                }
                                .w120{
                                    font-weight:bold;
                                    display:inline-block;
                                    width:120px;
                                    padding-right:3px
                                }
                            </style>
                                                        <meta http-equiv=""Content-Type"" content=""text/html;charset=utf-8""/>
                                                        <title>" + subject + @"</title></head>
                                                        <body bgcolor=""#8d8e90"">
                                                        <table width=""650"" border=""0"" cellspacing=""0"" cellpadding=""0"" bgcolor=""#FFFFFF"" align=""center"">
                         <tr><td  colspan=""3""><br/></td></tr>
                        <tr><td>
                        <table style=""width:100%"" border=""0"" cellspacing=""0"" cellpadding=""0"">
                                      <tr>
                                        <td width=""5%"">&nbsp;</td>
                                        <td width=""90%"" align=""left"" valign=""top"">"
                        + body
                        + "<br/><hr><p style='color:darkgrey'>Please do not reply to this email.This mailbox is not monitored and you will not receive a response."
                        + @"</td><td width=""5%"">&nbsp;</td></tr>
                         <tr><td  colspan=""3""><br/></td></tr><tr><td width=""5%"">&nbsp;</td><td> </td><td width=""5%"">&nbsp;</td></tr>
                         <tr><td width=""5%"">&nbsp;</td><td>"
                        + @"</td><td width=""5%"">&nbsp;</td></tr> <tr><td colspan=""3""><br/></td><tr> </table></td></tr></table></body></html>";
                mail.IsBodyHtml = true;
                mail.BodyEncoding = System.Text.Encoding.UTF8;

                if (string.IsNullOrWhiteSpace(fileAttach) == false)
                {
                    string[] attachFileArr = fileAttach.Split(new char[] { ';' });
                    foreach (var item in attachFileArr)
                    {
                        if (string.IsNullOrWhiteSpace(item) == false)
                        {
                            System.Net.Mail.Attachment attack = new System.Net.Mail.Attachment(item, MediaTypeNames.Application.Octet);
                            mail.Attachments.Add(attack);
                        }
                    }
                }
                await client.SendMailAsync(mail);
                client.Dispose();
                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> SendBySendGrid(string to, string name, string subject, string body, string cc = "", string bcc = "", bool template = true, string fileAttach = "")
        {
            try
            {
                if (!_enrichContext.IsProduction) to = ConfigurationManager.AppSettings["WebMail"];
                var client = new SendGridClient(_apikeySG);

                // Send a Single Email using the Mail Helper
                var fromEmail = new EmailAddress("dev@sposus.co", "Simply Pos");
                var htmlContent = "";
                if (template)
                {
                    htmlContent = @"<!DOCTYPE><html xmlns=""http://www.w3.org/1999/xhtml""><head>
                                    <style>
                                        .table_border, .table_border th, .table_border  td {
                                            border: 1px solid grey;
                                        }
                                    </style>
                                                                <meta http-equiv=""Content-Type"" content=""text/html;charset=utf-8""/>
                                                                <title>" + subject + @"</title></head>
                                                                <body  style='background-color:#8d8e90'><br/>
                                                                <table width=""650"" border=""0"" cellspacing=""0"" cellpadding=""0""  style='background-color:#ffffff ;' align=""center"">
                                 <tr><td  colspan=""3""><br/></td></tr>
                                <tr><td>
                                <table style=""width:100%"" border=""0"" cellspacing=""0"" cellpadding=""0"">
                                              <tr>
                                                <td width=""5%"">&nbsp;</td>
                                                <td width=""90%"" align=""left"" valign=""top"">"
                                + body
                                + "<br/><br/><br/><hr><p  style=\"color:darkgrey\" >Please do not reply to this email.This mailbox is not monitored and you will not receive a response.</p>"
                                + @"</td><td width=""5%"">&nbsp;</td></tr>
                                 <tr><td  colspan=""3""><br/></td></tr><tr><td width=""5%"">&nbsp;</td><td>  </td><td width=""5%"">&nbsp;</td></tr>
                                 <tr><td width=""5%"">&nbsp;</td><td>"
                                + @"</td></td><td width=""5%"">&nbsp;</td></tr> <tr><td colspan=""3""><br/></td><tr> </table></td></tr></table></body></html>";

                }
                else
                {
                    htmlContent = body;
                }

                string[] arrTo = to?.Split(new char[] { ';' });
                string[] arr_fName = name?.Split(new char[] { ';' });
                List<EmailAddress> emails = new List<EmailAddress>();
                for (int i = 0; i < arrTo.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(arrTo[i]) == false && emails.Any(e => e.Email == arrTo[i]) == false)
                    {
                        var toEmail = new EmailAddress(arrTo[i], arr_fName[i] ?? "Enrich");
                        emails.Add(toEmail);
                    }
                }

                var msg = MailHelper.CreateSingleEmailToMultipleRecipients(fromEmail, emails, subject, "", htmlContent, true);
                if (!string.IsNullOrWhiteSpace(cc))
                {
                    //cc
                    string[] ccArr = cc.Split(new char[] { ';' });
                    foreach (var item in ccArr)
                    {
                        if (string.IsNullOrWhiteSpace(item) == false && to.Contains(item) == false)
                        {
                            msg.AddCc(item);
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(bcc))
                {
                    //bcc
                    if (string.IsNullOrWhiteSpace(bcc) == false)
                    {
                        string[] bccArr = bcc.Split(new char[] { ';' });
                        foreach (var item in bccArr)
                        {
                            if (string.IsNullOrWhiteSpace(item) == false && to.Contains(item) == false)
                            {
                                msg.AddBcc(item);
                            }
                        }
                    }
                }
                if (string.IsNullOrWhiteSpace(fileAttach) == false)
                {
                    string[] attachFileArr = fileAttach.Split(new char[] { ';' });
                    foreach (var item in attachFileArr)
                    {
                        if (string.IsNullOrWhiteSpace(item) == false)
                        {
                            msg.AddAttachment(item.Split('\\').Last(), Convert.ToBase64String(File.ReadAllBytes(item)));
                        }
                    }
                }
                var a = Newtonsoft.Json.JsonConvert.SerializeObject(msg);
                var response = await client.SendEmailAsync(msg);
                if (response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.OK)
                {
                    return string.Empty;
                }
                else
                {
                    string x = msg.Serialize();
                    return msg.Serialize();
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        public async Task<string> SendBySendGridWithTemplate(string to, string firstName, string templateId, string cc, object data = null, string bcc = "")
        {
            try
            {
                if (!_enrichContext.IsProduction) to = ConfigurationManager.AppSettings["WebMail"];
                // Retrieve the API key from the environment variables. See the project README for more info about setting this up.
                var clientEmail = new SendGridClient(_apikeySG);

                // Send a Single Email using the Mail Helper
                var msg = new SendGridMessage();
                var fromEmail = new EmailAddress("dev@sposus.co", "@Simply Pos");
                msg.From = fromEmail;
                string[] arrTo = to?.Split(new char[] { ';' });
                string[] arr_fName = firstName?.Split(new char[] { ';' });

                List<EmailAddress> emails = new List<EmailAddress>();
                for (int i = 0; i < arrTo?.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(arrTo[i]) == false && emails.Any(e => e.Email == arrTo[i]) == false)
                    {
                        var toEmail = new EmailAddress(arrTo[i], arr_fName[i]);
                        emails.Add(toEmail);
                    }
                }

                msg.AddTos(emails);
                if (data != null)
                {
                    var dictObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(data));
                    dictObj.Add("productLogo", _url + sysConfig.ProductLogo);
                    dictObj.Add("supportEmail", sysConfig.SupportEmail);
                    dictObj.Add("supportPhone", sysConfig.CompanySupportNumber);
                    msg.SetTemplateData(dictObj);
                }

                if (!string.IsNullOrWhiteSpace(cc))
                {
                    //cc
                    string[] ccArr = cc.Split(new char[] { ';' });
                    foreach (var item in ccArr)
                    {
                        if (string.IsNullOrWhiteSpace(item) == false && to.Contains(item) == false)
                        {
                            msg.AddCc(item);
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(bcc))
                {
                    //bcc
                    if (string.IsNullOrWhiteSpace(bcc) == false)
                    {
                        string[] bccArr = bcc.Split(new char[] { ';' });
                        foreach (var item in bccArr)
                        {
                            if (string.IsNullOrWhiteSpace(item) == false && to.Contains(item) == false)
                            {
                                msg.AddBcc(item);
                            }
                        }
                    }
                }

                msg.TemplateId = templateId;
                var response = await clientEmail.SendEmailAsync(msg);
                if (response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.OK)
                {
                    return "";
                }
                else
                {
                    return msg.Serialize();
                }

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> SendBySendGrid_ReceiptPayment(string to, string name, object mailData, string cc = "", string bcc = "")
        {
            ReadXML xml = new ReadXML(System.Web.Hosting.HostingEnvironment.MapPath("/App_Data/Config.xml"));
            XmlNode node = xml.GetNode("/root/sendgrid_template/receipt_payment");
            return await SendBySendGridWithTemplate(to, name, node["template_id"].InnerText, cc, mailData, bcc);
        }

        public async Task SendEmailAfterChangedPass(string memberName, string username, string password, string to)
        {
            string body = "<p>Dear " + memberName + ", </p> " +
                        "<p>You are receiving this notification because you have changed password  in <a href='" + _domain + "'>" + _domain + "</a>.</p>" +
                        "<p>Your account information is as follows:</p>" +
                        "<p>Username: " + username + " </p>" +
                        "<p>password : " + password + " </p> " +
                        "<br/>" +

                       "Best Regards,<br/>- Support Team  <br/>";

            await SendBySendGrid(to, username, "[Spos]Your account password has changed", body);
        }

        public async Task<string> SendEmailAfterEmployeesCreated(string firstname, string username, string password, string partnerId, string cemail, string cepass, string to, string cc = "", string bcc = "")
        {
            string body = "<p>Dear " + firstname + ", </p>" +
                        "<p>Welcome Aboard and Thank You for registering to be part of SPOSUS.  Please keep this e-mail for your records.</p>" +
                        "<br/>" +
                        "<b>Your account information is as follows:</b><br/>" +
                        "---------------------------- <br/>" +
                        "<b class='w150'>Partner ID: </b>" + partnerId.ToUpper() + "<br/>" +
                        "<b class='w150'>Username: </b>" + username + "<br/>" +
                        "<b class='w150'>Password : </b>" + password + "<br/>";
            body += "<br/>Best Regards & Best of Lucks!<br/>" +
                  "- IMS Spos Team <br/>";

            return await SendBySendGrid(to, "", "[Spos]Welcome to Sposus Co", body, cc, bcc);
        }

        public async Task SendEmailAfterResetPass(string memberName, string username, string password, string to)
        {
            string body = "<p>Dear " + memberName + ", </p> " +
                        "<p>You are receiving this notification because your password has been resetted by admin in <a href='" + _domain + "'>" + _domain + "</a>.</p>" +
                        "<p>Your account information is as follows:</p>" +
                        "<p>Username: " + username + " </p>" +
                        "<p>password : " + password + " </p> " +
                        "<p>Please note that this password is being generated automatically. To ensure the accuracy of your information and privacy, " +
                        "please login to update and change your password at your earliest convenience.<br/><br/>" +
                       "Best Regards,<br/>- Support Team  <br/>";
            await SendBySendGrid(to, username, "[Spos]Your password has been resetted", body);
        }

        public async Task<string> SendEmailNotice(string body, string to, string firstname, string subject, string cc = "", string bcc = "")
        {
            string b = "Dear " + firstname + ",<br/>";
            b += body;
            b += "<br/><br/>- IMS Spos Team <br/>";
            return await Send(to, subject, b, bcc, cc);
        }

        public async Task<string> SendEmailNotice_Vi(string body, string to, string firstname, string subject, string cc = "", bool signature = true)
        {
            string b = "Xin chào " + firstname + ",<br/>";
            b += body;
            if (signature)
            {
                b += "<br/><br/>Phòng CSKH<br/>" + _domain + "<br/>";
            }
            return await Send(to, subject, b, "", cc);
        }

        public async Task<string> SendEmailToITNoticeNewStaff(string toITEmail, string firstName, string lastName, string cellphone, string printName, string cemail, string cepass, string title, string cc = "")
        {
            string body = "Dear IT, <br/>";
            body += "Please create the internal company email or update for this staff.<br/>" +
                    "<fieldset><legend>Employee infomation</legend><table>" +
                    "<tr><td style=\"Width:200px\">First name</td><td>" + firstName + "</td></tr>" +
                    "<tr><td style=\"Width:200px\">Last name</td><td>" + lastName + "</td></tr>" +
                    "<tr><td style=\"Width:200px\">Name on card</td><td>" + printName + "</td></tr>" +
                    "<tr><td style=\"Width:200px\">Title</td><td>" + title + "</td></tr>" +
                    "<tr><td style=\"Width:200px\">Cell Phone</td><td>" + cellphone + "</td></tr>" +
                    "</table></fieldset><br/>" +
                    "This is new staff's profile, Please create company email for this staff" +
                    "<br/><br/>Best Regards,<br/>" +
                    "- IMS Sposus Team <br/>";
            return await SendBySendGrid(toITEmail, "", "[Spos]Please create the new internal company email", body, cc, "");
        }

        public async Task<string> SendEmail_HR(string to, string firstName, string subject, string cc, object data = null, string fileAttach = "")
        {
            try
            {
                if (!_enrichContext.IsProduction) to = ConfigurationManager.AppSettings["WebMail"];
                // Retrieve the API key from the environment variables. See the project README for more info about setting this up.
                //var apiKey = Environment.GetEnvironmentVariable(_apinameSG);
                var clientEmail = new SendGridClient(_apikeySG);

                // Send a Single Email using the Mail Helper
                var msg = new SendGridMessage();
                var fromEmail = new EmailAddress("enrichhr@enrichco.us", "@Enrich HR");
                msg.From = fromEmail;
                string[] arrTo = to?.Split(new char[] { ';' });
                string[] arr_fName = firstName?.Split(new char[] { ';' });
                List<EmailAddress> emails = new List<EmailAddress>();
                for (int i = 0; i < arrTo?.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(arrTo[i]) == false && emails.Any(e => e.Email == arrTo[i]) == false)
                    {
                        var toEmail = new EmailAddress(arrTo[i], arr_fName[i]);
                        emails.Add(toEmail);
                    }
                }
                msg.Subject = subject;
                string replyto = _replyto;
                string replyname = _replyname;
                msg.ReplyTo = new EmailAddress(replyto, replyname);
                msg.AddTos(emails);
                if (data != null)
                {
                    msg.SetTemplateData(data);
                }

                if (!string.IsNullOrWhiteSpace(cc))
                {
                    //cc
                    string[] ccArr = cc.Split(new char[] { ';' });
                    foreach (var item in ccArr)
                    {
                        if (string.IsNullOrWhiteSpace(item) == false && to.Contains(item) == false)
                        {
                            msg.AddCc(item);
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(fileAttach) == false)
                {
                    string[] attachFileArr = fileAttach.Split(new char[] { '|' });
                    foreach (var item in attachFileArr)
                    {
                        if (string.IsNullOrWhiteSpace(item) == false)
                        {
                            string file = Path.Combine(HttpRuntime.AppDomainAppPath + item);
                            msg.AddAttachment(firstName + ".pdf", Convert.ToBase64String(File.ReadAllBytes(file)));
                        }
                    }
                }

                ReadXML xml = new ReadXML(System.Web.Hosting.HostingEnvironment.MapPath("/App_Data/Config.xml"));
                XmlNode node = xml.GetNode("/root/sendgrid_template/enrich_template");
                msg.TemplateId = node["template_id"].InnerText;
                var response = await clientEmail.SendEmailAsync(msg);
                if (response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.OK)
                {
                    return "";
                }
                else
                {
                    return msg.Serialize();
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> SendNotifyInstallationComplete(string to, string name, string bcc, SendGridEmailTemplateData.InstallCompleteTemplate EmailData)
        {
            ReadXML xml = new ReadXML(System.Web.Hosting.HostingEnvironment.MapPath("/App_Data/Config.xml"));
            XmlNode node = xml.GetNode("/root/sendgrid_template/notify_installation_complete");
            return await SendBySendGridWithTemplate(to, name, node["template_id"].InnerText, bcc, EmailData);
        }

        public async Task<string> SendNotifyOutgoingWithTemplate(string to, string firstName, string subject, string cc, object data = null, string fileAttach = "")
        {
            try
            {
                if (!_enrichContext.IsProduction) to = ConfigurationManager.AppSettings["WebMail"];
                // Retrieve the API key from the environment variables. See the project README for more info about setting this up.
                var clientEmail = new SendGridClient(_apikeySG);
                // Send a Single Email using the Mail Helper
                var msg = new SendGridMessage();
                var fromEmail = new EmailAddress("dev@sposus.co", "@Simply Pos");
                msg.From = fromEmail;
                string[] arrTo = to?.Split(new char[] { ';' });
                string[] arr_fName = firstName?.Split(new char[] { ';' });

                List<EmailAddress> emails = new List<EmailAddress>();
                for (int i = 0; i < arrTo?.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(arrTo[i]) == false && emails.Any(e => e.Email == arrTo[i]) == false)
                    {
                        var toEmail = new EmailAddress(arrTo[i], arr_fName[i]);
                        emails.Add(toEmail);
                    }
                }
                msg.Subject = subject;
                string replyto = _replyto;
                string replyname = _replyname;
                msg.ReplyTo = new EmailAddress(replyto, replyname);
                msg.AddTos(emails);

                if (data != null) msg.SetTemplateData(data);
                if (!string.IsNullOrWhiteSpace(cc))
                {
                    //cc
                    string[] ccArr = cc.Split(new char[] { ';' });
                    foreach (var item in ccArr)
                    {
                        if (string.IsNullOrWhiteSpace(item) == false && to.Contains(item) == false)
                        {
                            msg.AddCc(item);
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(fileAttach) == false)
                {
                    string[] attachFileArr = fileAttach.Split(new char[] { '|' });
                    foreach (var item in attachFileArr)
                    {
                        if (string.IsNullOrWhiteSpace(item) == false)
                        {
                            string file = Path.Combine(HttpRuntime.AppDomainAppPath + item);
                            msg.AddAttachment(file.Split('\\').Last(), Convert.ToBase64String(File.ReadAllBytes(file)));
                        }
                    }
                }

                ReadXML xml = new ReadXML(System.Web.Hosting.HostingEnvironment.MapPath("/App_Data/Config.xml"));
                XmlNode node = xml.GetNode("/root/sendgrid_template/customer_outgoing");
                msg.TemplateId = node["template_id"].InnerText;

                var response = await clientEmail.SendEmailAsync(msg);
                if (response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.OK)
                {
                    return string.Empty;
                }
                else
                {
                    return msg.Serialize();
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> SendNotifyResetPassword(string to, string name, string SalonName, string UserId, string UserPassword, string cc, string link = "", string bcc = "")
        {
            var email_data = new SendGridEmailTemplateData.ResetPasswordTemplate()
            {
                salon_name = SalonName,
                login_user = UserId,
                login_password = UserPassword,
                link = string.IsNullOrEmpty(link) ? null : new SendGridEmailTemplateData.Link()
                {
                    LinkChangePass = link
                }
            };
            ReadXML xml = new ReadXML(System.Web.Hosting.HostingEnvironment.MapPath("/App_Data/Config.xml"));
            XmlNode node = xml.GetNode("/root/sendgrid_template/notify_reset_password");
            return await SendBySendGridWithTemplate(to, name, node["template_id"].InnerText, cc, email_data, bcc);
        }

        public async Task<string> SendNotifySubmitEstimate(string to, string name, object mailData, string cc = "", string bcc = "")
        {
            ReadXML xml = new ReadXML(System.Web.Hosting.HostingEnvironment.MapPath("/App_Data/Config.xml"));
            XmlNode node = xml.GetNode("/root/sendgrid_template/salon_payment_confirm_and_pay");
            return await SendBySendGridWithTemplate(to, name, node["template_id"].InnerText, cc, mailData, bcc);
        }

        public async Task<string> SendEmailRequireGoogleAuth(string email)
        {
            try
            {
                var member = _unitOfWork.Repository<P_Member>().Table.FirstOrDefault(x => x.PersonalEmail == email);
                string emailEncrypt = SecurityLibrary.Encrypt(member.PersonalEmail);
                var url = _url + "/MemberMan/VerifyGoogleAuth?key=" + emailEncrypt;
                string subject = $"IMS Require Google Auth for email : {member.PersonalEmail}";
                string content = $"Dear {member.FullName},<br/><br/>" +
                                 $"<p>To make sure you're still using organizational email for your work.</p>" +
                                 $"<p>We need you to confirm by clicking the button below and login with enrich's email account.</p>" +
                                 $"<a style='background: #00c0ef; color: white; padding: 4px 18px; margin-top: 11px; display: inline-block; text-decoration: none; border-radius: 3px;' href=" + url + " style='margin-left: 25px;'><b>Verification</b></a><br/><br/>" +
                                 $"<p>Thank you for your cooperation !</p>";
                var emailData = new { content = content, subject = subject };
                var msg = await SendNotifyOutgoingWithTemplate(member.PersonalEmail, "", subject, "", emailData);
                member.IsSendEmailGoogleAuth = true;
                _unitOfWork.SaveChanges();
                return msg;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}

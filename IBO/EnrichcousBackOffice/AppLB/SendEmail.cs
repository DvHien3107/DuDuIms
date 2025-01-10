//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Net.Mail;
//using System.Net;
//using System.Net.Mime;
//using System.ComponentModel;
//using System.Configuration;
//using EnrichcousBackOffice.Models;
//using System.Threading.Tasks;
//using SendGrid;
//using SendGrid.Helpers.Mail;
//using System.Net.Http.Headers;
//using EnrichcousBackOffice.Models.CustomizeModel;
//using System.IO;
//using System.Xml;
//using System.Web.Mvc;
//using System.Web.Configuration;
//using Newtonsoft.Json;
//using Enrich.Core.Infrastructure;

//namespace EnrichcousBackOffice.AppLB
//{
//    public class SendEmail
//    {

//        #region send email function
//        private static string apiKey = ConfigurationManager.AppSettings["SendGridApiKey"];
//        private static readonly string MessageId = "X-Message-Id";

//        static string smtp = ConfigurationManager.AppSettings["SMTP"];
//        static string webmail = ConfigurationManager.AppSettings["WebMail"];
//        static string webmail_displayname = ConfigurationManager.AppSettings["WebMailDisplayName"];
//        static string webmailpass = ConfigurationManager.AppSettings["WebMailPass"];
//        static int webmailport = int.Parse(ConfigurationManager.AppSettings["WebMailPort"]);
//        static bool ssl = bool.Parse(ConfigurationManager.AppSettings["WebMailSSL"]);
//        public static string _domain = ConfigurationManager.AppSettings["Domain"];

//        private static WebDataModel db = new WebDataModel();
//        private static SystemConfiguration sysConfig = db.SystemConfigurations.FirstOrDefault();
//        //static string host = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
//        /// <summary>
//        /// Send email
//        /// </summary>
//        /// <param name="to"></param>
//        /// <param name="subject"></param>
//        /// <param name="body"></param>
//        /// <param name="bcc">chuoi cac email can bbc ngan cach boi dau ;</param>
//        /// <param name="fileAttach">duong dan tuong doi file dinh kem tren server</param>
//        /// <returns></returns>
//        public static async Task<string> Send(string to, string subject, string body, string bcc = "", string cc = "", string fileAttach = null)
//        {
//            //#if DEBUG
//            //            to = "noreply.enrichcous@gmail.coms";
//            //            cc = string.Empty;
//            //            bcc = string.Empty;
//            //#endif
//            try
//            {
//                SmtpClient client = new SmtpClient(smtp);
//                client.UseDefaultCredentials = false;
//                NetworkCredential nc = new NetworkCredential(webmail, webmailpass);
//                client.Credentials = nc;
//                client.Port = webmailport;
//                client.EnableSsl = ssl;


//                MailMessage mail = new MailMessage();
//                mail.From = new MailAddress(webmail, webmail_displayname);
//                mail.Sender = new MailAddress(webmail, webmail_displayname);
//                string[] arrTo = to.Split(new char[] { ';' });
//                for (int i = 0; i < arrTo.Length; i++)
//                {
//                    if (string.IsNullOrWhiteSpace(arrTo[i]) == false && mail.To.Any(e => e.Address.Equals(arrTo[i])) == false)
//                    {
//                        mail.To.Add(new MailAddress(arrTo[i]));
//                    }

//                }

//                if (string.IsNullOrWhiteSpace(bcc) == false)
//                {
//                    //bcc
//                    if (string.IsNullOrWhiteSpace(bcc) == false)
//                    {
//                        string[] ademail = bcc.Split(new char[] { ';' });
//                        foreach (var item in ademail)
//                        {
//                            if (string.IsNullOrWhiteSpace(item) == false && to.Contains(item) == false)
//                            {
//                                mail.Bcc.Add(new MailAddress(item));
//                            }
//                        }
//                    }


//                }
//                if (string.IsNullOrWhiteSpace(cc) == false)
//                {
//                    //cc
//                    string[] ccArr = cc.Split(new char[] { ';' });
//                    foreach (var item in ccArr)
//                    {
//                        if (string.IsNullOrWhiteSpace(item) == false && to.Contains(item) == false)
//                        {
//                            mail.CC.Add(new MailAddress(item));
//                        }
//                    }
//                }

//                SystemConfiguration webinfo = sysConfig;
//                mail.Subject = subject;
//                //mail.Body = "<html><header></header><body>" + body + "</body></html>";

//                mail.Body = @"<!DOCTYPE><html xmlns=""http://www.w3.org/1999/xhtml""><head>
//                            <style>
//                                .table_border, .table_border th, .table_border  td {
//                                    border: 1px solid grey;
//                                }
//                                .w150{
//                                    font-weight:bold;
//                                    display:inline-block;
//                                    width:150px;
//                                    padding-right:3px
//                                }
//                                .w120{
//                                    font-weight:bold;
//                                    display:inline-block;
//                                    width:120px;
//                                    padding-right:3px
//                                }
//                            </style>
//                                                        <meta http-equiv=""Content-Type"" content=""text/html;charset=utf-8""/>
//                                                        <title>" + subject + @"</title></head>
//                                                        <body bgcolor=""#8d8e90"">
//                                                        <table width=""650"" border=""0"" cellspacing=""0"" cellpadding=""0"" bgcolor=""#FFFFFF"" align=""center"">
//                         <tr><td  colspan=""3""><br/></td></tr>
//                        <tr><td>
//                        <table style=""width:100%"" border=""0"" cellspacing=""0"" cellpadding=""0"">
//                                      <tr>
//                                        <td width=""5%"">&nbsp;</td>
//                                        <td width=""90%"" align=""left"" valign=""top"">"
//                        + body
//                        + "<br/><hr><p style='color:darkgrey'>Please do not reply to this email.This mailbox is not monitored and you will not receive a response."
//                        + @"</td><td width=""5%"">&nbsp;</td></tr>
//                         <tr><td  colspan=""3""><br/></td></tr><tr><td width=""5%"">&nbsp;</td><td> </td><td width=""5%"">&nbsp;</td></tr>
//                         <tr><td width=""5%"">&nbsp;</td><td>"
//                        //+ @"<b>" + webinfo.CompanyName + "</b><br/>"
//                        //+ "Email: " + webinfo.SupportEmail + @"<br/>
//                        //            website:" + _domain + @"<br/>
//                        //            Hotline: " + webinfo.CompanyHotline + @"<br/>
//                        //            Adress: " + webinfo.CompanyAddress
//                        + @"</td><td width=""5%"">&nbsp;</td></tr> <tr><td colspan=""3""><br/></td><tr> </table></td></tr></table></body></html>";

//                mail.IsBodyHtml = true;
//                mail.BodyEncoding = System.Text.Encoding.UTF8;


//                if (string.IsNullOrWhiteSpace(fileAttach) == false)
//                {
//                    string[] attachFileArr = fileAttach.Split(new char[] { ';' });
//                    foreach (var item in attachFileArr)
//                    {
//                        if (string.IsNullOrWhiteSpace(item) == false)
//                        {
//                            System.Net.Mail.Attachment attack = new System.Net.Mail.Attachment(item, MediaTypeNames.Application.Octet);
//                            mail.Attachments.Add(attack);
//                        }
//                    }

//                }

//                await client.SendMailAsync(mail);

//                client.Dispose();

//                return "";
//            }
//            catch (Exception ex)
//            {
//                return ex.Message;
//            }



//        }

//        #endregion



//        /// <summary>
//        /// gui email thong bao
//        /// </summary>
//        /// <param name="body"></param>
//        /// <param name="to"></param>
//        /// <param name="fullname"></param>
//        /// <param name="subject"></param>
//        /// <param name="cc">email can cc</param>
//        /// <param name="signature">false: khong dung default footer.</param>
//        public static async Task<string> SendEmailNotice_Vi(string body, string to, string firstname, string subject, string cc = "", bool signature = true)
//        {
//            string b = "Xin chào " + firstname + ",<br/>";
//            b += body;
//            if (signature)
//            {
//                b += "<br/><br/>Phòng CSKH<br/>" + _domain + "<br/>";
//            }
//            return await Send(to, subject, b, "", cc);
//        }


//        public static async Task<string> SendEmailNotice(string body, string to, string firstname, string subject, string cc = "", string bcc = "")
//        {
//            string b = "Dear " + firstname + ",<br/>";
//            b += body;
//            //if (signature)
//            //{
//            b += "<br/><br/>- IMS Enrich Team <br/>";
//            //}
//            return await Send(to, subject, b, bcc, cc);
//        }


//        /// <summary>
//        ///  Email notice when password has changed
//        /// </summary>
//        public static async Task SendEmailAfterChangedPass(string memberName, string username, string password, string to)
//        {
//            string body = "<p>Dear " + memberName + ", </p> " +
//                        "<p>You are receiving this notification because you have changed password  in <a href='" + _domain + "'>" + _domain + "</a>.</p>" +
//                        "<p>Your account information is as follows:</p>" +
//                        "<p>Username: " + username + " </p>" +
//                        "<p>password : " + password + " </p> " +
//                        "<br/>" +

//                       "Best Regards,<br/>- Support Team  <br/>";

//            await SendBySendGrid(to, username, "[Enrich]Your account password has changed", body);
//        }


//        /// <summary>
//        /// Gui email sau khi reset pass
//        /// </summary>
//        public static async Task SendEmailAfterResetPass(string memberName, string username, string password, string to)
//        {
//            string body = "<p>Dear " + memberName + ", </p> " +
//                        "<p>You are receiving this notification because your password has been resetted by admin in <a href='" + _domain + "'>" + _domain + "</a>.</p>" +
//                        "<p>Your account information is as follows:</p>" +
//                        "<p>Username: " + username + " </p>" +
//                        "<p>password : " + password + " </p> " +
//                        "<p>Please note that this password is being generated automatically. To ensure the accuracy of your information and privacy, " +
//                        "please login to update and change your password at your earliest convenience.<br/><br/>" +

//                       "Best Regards,<br/>- Support Team  <br/>";

//            await SendBySendGrid(to, username, "[Enrich]Your password has been resetted", body);

//        }



//        /// <summary>
//        /// gui email thong bao den nhan vien sau khi tk cua ho duoc tao
//        /// </summary>
//        public static async Task<string> SendEmailAfterEmployeesCreated(string firstname, string username, string password, string partnerId, string cemail, string cepass, string to, string cc = "", string bcc = "")
//        {
//            string body = "<p>Dear " + firstname + ", </p>" +
//                        "<p>Welcome Aboard and Thank You for registering to be part of ENRICHCO.  Please keep this e-mail for your records.</p>" +
//                        "<br/>" +
//                        "<b>Your account information is as follows:</b><br/>" +
//                        "---------------------------- <br/>" +
//                        "<b class='w150'>Partner ID: </b>" + partnerId.ToUpper() + "<br/>" +
//                        "<b class='w150'>Username: </b>" + username + "<br/>" +
//                        "<b class='w150'>Password : </b>" + password + "<br/>";
//            //"<b class='w150'>Company email: </b>" + cemail + "<br/>" +
//            //"<b class='w150'>Temp password : </b>" + cepass + "<br/><br/>";

//            body += "<br/>Best Regards & Best of Lucks!<br/>" +
//                  "- IMS Enrich Team <br/>";

//            return await SendBySendGrid(to, "", "[Enrich]Welcome to ENRICH & CO", body, cc, bcc);

//        }

//        /// <summary>
//        /// gui email den IT thong bao tao companyemail cho nhan vien
//        /// <paramref name="listEmail">List<Dictionary<string,string>> ["email","pass"]</paramref>
//        /// </summary>
//        public static async Task<string> SendEmailToITNoticeNewStaff(string toITEmail, string firstName, string lastName, string cellphone, string printName, string cemail, string cepass, string title, string cc = "")
//        {

//            string body = "Dear  IT, <br/>";
//            body += "Please create the internal company email or update for this staff.<br/>";
//            body += "<fieldset><legend>Employee infomation</legend><table>" +
//                    "<tr><td style=\"Width:200px\">First name</td><td>" + firstName + "</td></tr>" +
//                    "<tr><td style=\"Width:200px\">Last name</td><td>" + lastName + "</td></tr>" +
//                    "<tr><td style=\"Width:200px\">Name on card</td><td>" + printName + "</td></tr>" +
//                    "<tr><td style=\"Width:200px\">Title</td><td>" + title + "</td></tr>";
//            // "<tr><td style=\"Width:200px\">Company email</td><td style=\"font-weight:bold\">" + cemail + "</td></tr>" +
//            //"<tr><td style=\"Width:200px\">Temp pass</td><td>" + cepass + "</td></tr>";

//            body += "<tr><td style=\"Width:200px\">Cell Phone</td><td>" + cellphone + "</td></tr>" +
//                   "</table></fieldset><br/>";
//            body += "This is new staff's profile, Please create company email for this staff";
//            body += "<br/><br/>Best Regards,<br/>" +
//               "- IMS Enrich Team <br/>";

//            return await SendBySendGrid(toITEmail, "", "[Enrich]Please create the new internal company email", body, cc, "");
//        }



//        #region Send Email Using SendGrid



//        static string _apikeySG = ConfigurationManager.AppSettings["SendGridApiKey"];



//        /// <summary>
//        /// Send email
//        /// </summary>
//        /// <param name="to">cach bang dau ; neu co nhieu hon 1 email</param>
//		///<param name="name">cach bang dau ; neu co nhieu hon 1 email</param>
//        /// <param name="subject"></param>
//        /// <param name="body"></param>
//        /// <param name="bcc">chuoi cac email can bbc ngan cach boi dau ;</param>
//        /// <returns></returns>
//        public static async Task<string> SendBySendGrid(string to, string name, string subject, string body, string cc = "", string bcc = "", bool template = true, string fileAttach = "")
//        {
//            //#if DEBUG
//            //            to = "noreply.enrichcous@gmail.com";
//            //            name = to;
//            //            cc = string.Empty;
//            //            bcc = string.Empty;
//            //#endif
//            try
//            {
//                //to = "sonht10@yahoo.com";
//                // Retrieve the API key from the environment variables. See the project README for more info about setting this up.
//                //var apiKey = Environment.GetEnvironmentVariable(_apinameSG);

//                var client = new SendGridClient(_apikeySG);

//                // Send a Single Email using the Mail Helper
//                var fromEmail = new EmailAddress("info@enrichco.us", "@enrich");

//                var htmlContent = "";
//                if (template)
//                {
//                    htmlContent = @"<!DOCTYPE><html xmlns=""http://www.w3.org/1999/xhtml""><head>
//                                    <style>
//                                        .table_border, .table_border th, .table_border  td {
//                                            border: 1px solid grey;
//                                        }
//                                    </style>
//                                                                <meta http-equiv=""Content-Type"" content=""text/html;charset=utf-8""/>
//                                                                <title>" + subject + @"</title></head>
//                                                                <body  style='background-color:#8d8e90'><br/>
//                                                                <table width=""650"" border=""0"" cellspacing=""0"" cellpadding=""0""  style='background-color:#ffffff ;' align=""center"">
//                                 <tr><td  colspan=""3""><br/></td></tr>
//                                <tr><td>
//                                <table style=""width:100%"" border=""0"" cellspacing=""0"" cellpadding=""0"">
//                                              <tr>
//                                                <td width=""5%"">&nbsp;</td>
//                                                <td width=""90%"" align=""left"" valign=""top"">"
//                                + body
//                                + "<br/><br/><br/><hr><p  style=\"color:darkgrey\" >Please do not reply to this email.This mailbox is not monitored and you will not receive a response.</p>"
//                                + @"</td><td width=""5%"">&nbsp;</td></tr>
//                                 <tr><td  colspan=""3""><br/></td></tr><tr><td width=""5%"">&nbsp;</td><td>  </td><td width=""5%"">&nbsp;</td></tr>
//                                 <tr><td width=""5%"">&nbsp;</td><td>"
//                                + @"</td></td><td width=""5%"">&nbsp;</td></tr> <tr><td colspan=""3""><br/></td><tr> </table></td></tr></table></body></html>";

//                }
//                else
//                {
//                    htmlContent = body;
//                }



//                string[] arrTo = to?.Split(new char[] { ';' });
//                string[] arr_fName = name?.Split(new char[] { ';' });

//                List<EmailAddress> emails = new List<EmailAddress>();
//                for (int i = 0; i < arrTo.Length; i++)
//                {
//                    if (string.IsNullOrWhiteSpace(arrTo[i]) == false && emails.Any(e => e.Email == arrTo[i]) == false)
//                    {
//                        var toEmail = new EmailAddress(arrTo[i], arr_fName[i] ?? "Enrich");
//                        emails.Add(toEmail);
//                    }
//                }

//                var msg = MailHelper.CreateSingleEmailToMultipleRecipients(fromEmail, emails, subject, "", htmlContent, true);
//                if (!string.IsNullOrWhiteSpace(cc))
//                {
//                    //cc
//                    string[] ccArr = cc.Split(new char[] { ';' });
//                    foreach (var item in ccArr)
//                    {
//                        if (string.IsNullOrWhiteSpace(item) == false && to.Contains(item) == false)
//                        {
//                            msg.AddCc(item);
//                        }
//                    }
//                }

//                if (!string.IsNullOrWhiteSpace(bcc))
//                {
//                    //bcc
//                    if (string.IsNullOrWhiteSpace(bcc) == false)
//                    {
//                        string[] bccArr = bcc.Split(new char[] { ';' });
//                        foreach (var item in bccArr)
//                        {
//                            if (string.IsNullOrWhiteSpace(item) == false && to.Contains(item) == false)
//                            {
//                                msg.AddBcc(item);
//                            }
//                        }
//                    }
//                }
//                if (string.IsNullOrWhiteSpace(fileAttach) == false)
//                {
//                    string[] attachFileArr = fileAttach.Split(new char[] { ';' });
//                    foreach (var item in attachFileArr)
//                    {
//                        if (string.IsNullOrWhiteSpace(item) == false)
//                        {
//                            msg.AddAttachment(item.Split('\\').Last(), Convert.ToBase64String(File.ReadAllBytes(item)));
//                        }
//                    }

//                }
//                var a = Newtonsoft.Json.JsonConvert.SerializeObject(msg);
//                var response = await client.SendEmailAsync(msg);


//                //Console.WriteLine(msg.Serialize());
//                //Console.WriteLine("SendBySendGrid: " + response.StatusCode);
//                if (response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.OK)
//                {
//                    return "";
//                }
//                else
//                {
//                    return msg.Serialize();
//                }

//            }
//            catch (Exception ex)
//            {
//                return ex.Message;
//            }



//        }

//        /// <summary>
//        /// Send email
//        /// </summary>
//        /// <param name="to">cach bang dau ; neu co nhieu hon 1 email</param>
//        ///<param name="firstName">cach bang dau ; neu co nhieu hon 1 email</param>
//        /// <param name="data"></param>
//        /// <param name="bcc">chuoi cac email can bbc ngan cach boi dau ;</param>
//        /// <param name="templateId"></param>
//        /// <returns></returns>
//        public static async Task<string> SendBySendGridWithTemplate(string to, string firstName, string templateId, string cc, object data = null, string bcc = "")
//        {
//#if DEBUG
//            to = "noreply.enrichcous@gmail.com";
//            cc = string.Empty;
//            bcc = string.Empty;
//#endif
//            try
//            {
//                // Retrieve the API key from the environment variables. See the project README for more info about setting this up.
//                //var apiKey = Environment.GetEnvironmentVariable(_apinameSG);
//                var clientEmail = new SendGridClient(_apikeySG);

//                // Send a Single Email using the Mail Helper
//                var msg = new SendGridMessage();
//                var fromEmail = new EmailAddress("info@enrichco.us", "@Simply Pos");
//                msg.From = fromEmail;

//                //khong phai production thi gui mail cho IMS dev
//                //if (!EngineContext.Current.Resolve<EnrichContext>().IsProduction) to = ConfigurationManager.AppSettings["WebMail"];

//                string[] arrTo = to?.Split(new char[] { ';' });
//                string[] arr_fName = firstName?.Split(new char[] { ';' });

//                List<EmailAddress> emails = new List<EmailAddress>();
//                for (int i = 0; i < arrTo?.Length; i++)
//                {
//                    if (string.IsNullOrWhiteSpace(arrTo[i]) == false && emails.Any(e => e.Email == arrTo[i]) == false)
//                    {
//                        var toEmail = new EmailAddress(arrTo[i], arr_fName[i]);
//                        emails.Add(toEmail);
//                    }
//                }

//                msg.AddTos(emails);

//                if (data != null)
//                {
//                    var dictObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(data));
//                    dictObj.Add("productLogo", ConfigurationManager.AppSettings["IMSUrl"] + sysConfig.ProductLogo);
//                    dictObj.Add("supportEmail", sysConfig.SupportEmail);
//                    dictObj.Add("supportPhone", sysConfig.CompanySupportNumber);
//                    msg.SetTemplateData(dictObj);
//                }

//                if (!string.IsNullOrWhiteSpace(cc))
//                {
//                    //cc
//                    string[] ccArr = cc.Split(new char[] { ';' });
//                    foreach (var item in ccArr)
//                    {
//                        if (string.IsNullOrWhiteSpace(item) == false && to.Contains(item) == false)
//                        {
//                            msg.AddCc(item);
//                        }
//                    }
//                }

//                if (!string.IsNullOrWhiteSpace(bcc))
//                {
//                    //bcc
//                    if (string.IsNullOrWhiteSpace(bcc) == false)
//                    {
//                        string[] bccArr = bcc.Split(new char[] { ';' });
//                        foreach (var item in bccArr)
//                        {
//                            if (string.IsNullOrWhiteSpace(item) == false && to.Contains(item) == false)
//                            {
//                                msg.AddBcc(item);
//                            }
//                        }
//                    }
//                }

//                msg.TemplateId = templateId;

//                var response = await clientEmail.SendEmailAsync(msg);
//                //Console.WriteLine(msg.Serialize());
//                //Console.WriteLine("SendBySendGridWithTemplate: " + response.StatusCode);
//                if (response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.OK)
//                {
//                    return "";
//                }
//                else
//                {
//                    return msg.Serialize();
//                }

//            }
//            catch (Exception ex)
//            {
//                return ex.Message;
//            }



//        }

//        public static async Task<string> SendNotifyResetPassword(string to, string name, string SalonName, string UserId, string UserPassword, string cc, string link = "", string bcc = "")
//        {
//            var email_data = new SendGridEmailTemplateData.ResetPasswordTemplate()
//            {
//                salon_name = SalonName,
//                login_user = UserId,
//                login_password = UserPassword,
//                link = string.IsNullOrEmpty(link) ? null : new SendGridEmailTemplateData.Link()
//                {
//                    LinkChangePass = link
//                }
//            };
//            ReadXML xml = new ReadXML(System.Web.Hosting.HostingEnvironment.MapPath("/App_Data/Config.xml"));
//            XmlNode node = xml.GetNode("/root/sendgrid_template/notify_reset_password");
//            return await SendBySendGridWithTemplate(to, name, node["template_id"].InnerText, cc, email_data, bcc);
//        }

//        public static async Task<string> SendNotifySubmitEstimate(string to, string name, object mailData, string cc = "", string bcc = "")
//        {
//            ReadXML xml = new ReadXML(System.Web.Hosting.HostingEnvironment.MapPath("/App_Data/Config.xml"));
//            XmlNode node = xml.GetNode("/root/sendgrid_template/salon_payment_confirm_and_pay");
//            return await SendBySendGridWithTemplate(to, name, node["template_id"].InnerText, cc, mailData, bcc);
//        }

//        /// <summary>
//        /// Gửi email khi Installation Complete (active store_license)
//        /// </summary>
//        /// <param name="to">Email salon</param>
//        /// <param name="name">Sender name</param>
//        /// <param name="bcc">bcc</param>
//        /// <param name="EmailData">Format : SendGridEmailTemplateData.InstallCompleteTemplate</param>
//        /// <returns></returns>
//        public static async Task<string> SendNotifyInstallationComplete(string to, string name, string bcc, SendGridEmailTemplateData.InstallCompleteTemplate EmailData)
//        {
//            ReadXML xml = new ReadXML(System.Web.Hosting.HostingEnvironment.MapPath("/App_Data/Config.xml"));
//            XmlNode node = xml.GetNode("/root/sendgrid_template/notify_installation_complete");
//            return await SendBySendGridWithTemplate(to, name, node["template_id"].InnerText, bcc, EmailData);
//        }
//        public static async Task<string> SendBySendGrid_ReceiptPayment(string to, string name, object mailData, string cc = "", string bcc = "")
//        {
//            ReadXML xml = new ReadXML(System.Web.Hosting.HostingEnvironment.MapPath("/App_Data/Config.xml"));
//            XmlNode node = xml.GetNode("/root/sendgrid_template/receipt_payment");
//            return await SendBySendGridWithTemplate(to, name, node["template_id"].InnerText, cc, mailData, bcc);
//        }
//        //public static EmailResponse SendEmailGrid()
//        //{
//        //    string FromEmailAddress = "locdv95@gmail.com";
//        //    string ToEmailAddress = "locdv95@gmail.com";
//        //    string Subject = "Test SendGrid";
//        //    string Body = "Hello Loc Dinh";
//        //    string BccEmailAddress = "";
//        //    string CcEmailAddress = "";

//        //    var _client = new SendGridClient(apiKey);
//        //    var emailMessage = new SendGridMessage()
//        //    {
//        //        From = new EmailAddress(FromEmailAddress, "Sender"),
//        //        Subject = Subject,
//        //        HtmlContent = Body
//        //    };

//        //    emailMessage.AddTo(new EmailAddress(ToEmailAddress, "TextUser"));
//        //    if (!string.IsNullOrWhiteSpace(BccEmailAddress))
//        //    {
//        //        emailMessage.AddBcc(new EmailAddress(BccEmailAddress));
//        //    }

//        //    if (!string.IsNullOrWhiteSpace(CcEmailAddress))
//        //    {
//        //        emailMessage.AddBcc(new EmailAddress(CcEmailAddress));
//        //    }

//        //    return ProcessResponse(_client.SendEmailAsync(emailMessage).Result);
//        //}


//        //private static EmailResponse ProcessResponse(Response response)
//        //{
//        //    if (response.StatusCode.Equals(System.Net.HttpStatusCode.Accepted)
//        //        || response.StatusCode.Equals(System.Net.HttpStatusCode.OK))
//        //    {
//        //        return ToMailResponse(response);
//        //    }

//        //    //TODO check for null
//        //    var errorResponse = response.Body.ReadAsStringAsync().Result;

//        //    throw new EmailServiceException(response.StatusCode.ToString(), errorResponse);
//        //}


//        //private static EmailResponse ToMailResponse(Response response)
//        //{
//        //    if (response == null)
//        //        return null;

//        //    var headers = (HttpHeaders)response.Headers;
//        //    var messageId = headers.GetValues(MessageId).FirstOrDefault();
//        //    return new EmailResponse()
//        //    {
//        //        UniqueMessageId = messageId,
//        //        DateSent = DateTime.UtcNow,
//        //    };
//        //}

//        #endregion


//        public static async Task<string> SendNotifyOutgoingWithTemplate(string to, string firstName, string subject, string cc, object data = null, string fileAttach = "")
//        {
//            try
//            {
//                // Retrieve the API key from the environment variables. See the project README for more info about setting this up.
//                //var apiKey = Environment.GetEnvironmentVariable(_apinameSG);
//                var clientEmail = new SendGridClient(_apikeySG);

//                // Send a Single Email using the Mail Helper
//                var msg = new SendGridMessage();
//                var fromEmail = new EmailAddress("info@enrichco.us", "@Simply Pos");
//                msg.From = fromEmail;

//                //khong phai production thi gui mail cho IMS dev
//                //if (!EngineContext.Current.Resolve<EnrichContext>().IsProduction) to = ConfigurationManager.AppSettings["WebMail"];

//                string[] arrTo = to?.Split(new char[] { ';' });
//                string[] arr_fName = firstName?.Split(new char[] { ';' });

//                List<EmailAddress> emails = new List<EmailAddress>();
//                for (int i = 0; i < arrTo?.Length; i++)
//                {
//                    if (string.IsNullOrWhiteSpace(arrTo[i]) == false && emails.Any(e => e.Email == arrTo[i]) == false)
//                    {
//                        var toEmail = new EmailAddress(arrTo[i], arr_fName[i]);
//                        emails.Add(toEmail);
//                    }
//                }
//                msg.Subject = subject;

//                string replyto = WebConfigurationManager.AppSettings["ReplyEmail"];
//                string replyname = WebConfigurationManager.AppSettings["ReplyName"];
//                msg.ReplyTo = new EmailAddress(replyto, replyname);

//                msg.AddTos(emails);

//                if (data != null)
//                {
//                    msg.SetTemplateData(data);
//                }

//                if (!string.IsNullOrWhiteSpace(cc))
//                {
//                    //cc
//                    string[] ccArr = cc.Split(new char[] { ';' });
//                    foreach (var item in ccArr)
//                    {
//                        if (string.IsNullOrWhiteSpace(item) == false && to.Contains(item) == false)
//                        {
//                            msg.AddCc(item);
//                        }
//                    }
//                }

//                if (string.IsNullOrWhiteSpace(fileAttach) == false)
//                {
//                    string[] attachFileArr = fileAttach.Split(new char[] { '|' });
//                    foreach (var item in attachFileArr)
//                    {
//                        if (string.IsNullOrWhiteSpace(item) == false)
//                        {
//                            string file = Path.Combine(HttpRuntime.AppDomainAppPath + item);
//                            msg.AddAttachment(file.Split('\\').Last(), Convert.ToBase64String(File.ReadAllBytes(file)));
//                        }
//                    }

//                }

//                ReadXML xml = new ReadXML(System.Web.Hosting.HostingEnvironment.MapPath("/App_Data/Config.xml"));
//                XmlNode node = xml.GetNode("/root/sendgrid_template/customer_outgoing");
//                msg.TemplateId = node["template_id"].InnerText;

//                var response = await clientEmail.SendEmailAsync(msg);
//                //Console.WriteLine(msg.Serialize());
//                //Console.WriteLine("SendBySendGridWithTemplate: " + response.StatusCode);
//                if (response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.OK)
//                {
//                    return "";
//                }
//                else
//                {
//                    return msg.Serialize();
//                }

//            }
//            catch (Exception ex)
//            {
//                return ex.Message;
//            }



//        }

//        public static async Task<string> SendEmail_HR(string to, string firstName, string subject, string cc, object data = null, string fileAttach = "")
//        {
//            try
//            {
//                // Retrieve the API key from the environment variables. See the project README for more info about setting this up.
//                //var apiKey = Environment.GetEnvironmentVariable(_apinameSG);
//                var clientEmail = new SendGridClient(_apikeySG);

//                // Send a Single Email using the Mail Helper
//                var msg = new SendGridMessage();
//                var fromEmail = new EmailAddress("enrichhr@enrichco.us", "@Enrich HR");
//                msg.From = fromEmail;

//                //khong phai production thi gui mail cho IMS dev
//                //if (!EngineContext.Current.Resolve<EnrichContext>().IsProduction) to = ConfigurationManager.AppSettings["WebMail"];

//                string[] arrTo = to?.Split(new char[] { ';' });
//                string[] arr_fName = firstName?.Split(new char[] { ';' });

//                List<EmailAddress> emails = new List<EmailAddress>();
//                for (int i = 0; i < arrTo?.Length; i++)
//                {
//                    if (string.IsNullOrWhiteSpace(arrTo[i]) == false && emails.Any(e => e.Email == arrTo[i]) == false)
//                    {
//                        var toEmail = new EmailAddress(arrTo[i], arr_fName[i]);
//                        emails.Add(toEmail);
//                    }
//                }
//                msg.Subject = subject;

//                string replyto = WebConfigurationManager.AppSettings["ReplyEmail"];
//                string replyname = WebConfigurationManager.AppSettings["ReplyName"];
//                msg.ReplyTo = new EmailAddress(replyto, replyname);

//                msg.AddTos(emails);

//                if (data != null)
//                {
//                    msg.SetTemplateData(data);
//                }

//                if (!string.IsNullOrWhiteSpace(cc))
//                {
//                    //cc
//                    string[] ccArr = cc.Split(new char[] { ';' });
//                    foreach (var item in ccArr)
//                    {
//                        if (string.IsNullOrWhiteSpace(item) == false && to.Contains(item) == false)
//                        {
//                            msg.AddCc(item);
//                        }
//                    }
//                }

//                if (string.IsNullOrWhiteSpace(fileAttach) == false)
//                {
//                    string[] attachFileArr = fileAttach.Split(new char[] { '|' });
//                    foreach (var item in attachFileArr)
//                    {
//                        if (string.IsNullOrWhiteSpace(item) == false)
//                        {
//                            string file = Path.Combine(HttpRuntime.AppDomainAppPath + item);
//                            msg.AddAttachment(firstName + ".pdf", Convert.ToBase64String(File.ReadAllBytes(file)));
//                        }
//                    }

//                }

//                ReadXML xml = new ReadXML(System.Web.Hosting.HostingEnvironment.MapPath("/App_Data/Config.xml"));
//                XmlNode node = xml.GetNode("/root/sendgrid_template/enrich_template");
//                msg.TemplateId = node["template_id"].InnerText;

//                var response = await clientEmail.SendEmailAsync(msg);
//                //Console.WriteLine(msg.Serialize());
//                //Console.WriteLine("SendBySendGridWithTemplate: " + response.StatusCode);
//                if (response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.OK)
//                {
//                    return "";
//                }
//                else
//                {
//                    return msg.Serialize();
//                }

//            }
//            catch (Exception ex)
//            {
//                return ex.Message;
//            }



//        }
//    }
//}
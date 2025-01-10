using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;

namespace Promotion.Mango.Services
{
    public class SendEmailService
    {
        public  async Task<string> SendEmailContact(string to, string firstName, string subject, string cc, object data = null, string fileAttach = "")
        {
            try
            {
                // Retrieve the API key from the environment variables. See the project README for more info about setting this up.
                //var apiKey = Environment.GetEnvironmentVariable(_apinameSG);
                var apiKey = WebConfigurationManager.AppSettings["SendGridApiKey"];
                var clientEmail = new SendGridClient(apiKey);

                // Send a Single Email using the Mail Helper
                var msg = new SendGridMessage();
                var fromEmail = new EmailAddress("info@enrichco.us", "@Simply Pos");
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
                            msg.AddAttachment(file.Split('\\').Last(), Convert.ToBase64String(File.ReadAllBytes(file)));
                        }
                    }

                }

                msg.TemplateId = WebConfigurationManager.AppSettings["TemplateId"];

                var response = await clientEmail.SendEmailAsync(msg);
                //Console.WriteLine(msg.Serialize());
                //Console.WriteLine("SendBySendGridWithTemplate: " + response.StatusCode);
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
    }
}
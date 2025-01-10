using DocuSign.eSign.Model;
using Enrich.IServices.Utils.Mailing;
using Enrich.Services.Utils.Mailing;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.NextGen;
using EnrichcousBackOffice.Services.Repository;
using EnrichcousBackOffice.ViewControler;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace EnrichcousBackOffice.Services.NextGen.Mail
{
    public interface IEmailService
    {
        Task sendEmailInvoice(C_Customer cus, O_Orders order, string urlPaymentLink);
    }
    public class EmailService: IEmailService
    {
        private readonly string NextGenDomain = DomainConfig.NextGenApi;
        private readonly IOrderRepository _orderResponsitory;
        private readonly IMailingService _mailService;
        public EmailService(IOrderRepository orderRepository, ICustomerRepository cusResponsitory, IMailingService mailService) {
            _orderResponsitory = orderRepository;
            _mailService = mailService;
        }

        public async Task sendEmailInvoice(C_Customer cus, O_Orders order, string urlPaymentLink)
        {
            try
            {
                string InvoicePayment = AppDomain.CurrentDomain.BaseDirectory + "\\Template\\InvoicePayment.html";
                string message = System.IO.File.ReadAllText(InvoicePayment);
                var Orders = _orderResponsitory.GetOrderDetailByOrderCode(order.OrdersCode);
                WebDataModel db = new WebDataModel();
                var company_info = db.SystemConfigurations.FirstOrDefault();
                string supportPhone = company_info.CompanySupportNumber;
                try
                {
                    supportPhone = String.Format("{0:(###) ###-####}", company_info.CompanySupportNumber);
                }
                catch { }

                message = message
                    .Replace("[StoreName]", order.CustomerName)
                    .Replace("[TotalAmount]", order.GrandTotal.ToString())
                    .Replace("[PayHref]", urlPaymentLink)
                    .Replace("[BillingMail]", company_info.BillingNotification)
                    .Replace("[CallInfo]", $"at {supportPhone} from 9:00 AM – 9:00 PM EST.");
                string html = "";
                foreach (var o in Orders)
                {
                    html += $"<tr><td style=\"padding:10px;text-align:left\">{o.ProductName}</td><td style=\"padding:10px;text-align:right\">${o.TotalAmount}</td></tr>";
                }
                message = message.Replace("[ServiceList]", html).Replace(Environment.NewLine,"");
                await SendEmail(cus.Email, "Invoice is generated.", message);
            } catch (Exception ex)
            {

            }
           
        }

        public async Task SendEmail(string sendTo, string subject, string body)
        {

            await _mailService.SendBySendGrid(sendTo,"", subject, body);
            //var client = new HttpClient();
            //body = HttpUtility.UrlEncode(body);
            //sendTo = HttpUtility.UrlEncode(sendTo);
            //subject = HttpUtility.UrlEncode(subject);
            //var request = new HttpRequestMessage(HttpMethod.Put, $"{NextGenDomain}/SendMail?sendTo={sendTo}&subject={subject}&body={body}");
            //var response = await client.SendAsync(request);
            //response.EnsureSuccessStatusCode();
            //Console.WriteLine(await response.Content.ReadAsStringAsync());
        }

    }
}

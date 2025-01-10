using Microsoft.AspNetCore.Mvc;
using Pos.Model.Model.Comon;
using Pos.Model.Model.Respons;
using SendGrid.Helpers.Mail;
using SendGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Pos.Application.Repository;

namespace Pos.Application.Services.Scoped
{
    public interface IEmailService
    {
        void SendMail(string SendTo, string Subject, string Body); 
        Task<EmailResponse> sendEmail(EmailContract contract);
    }
    public class EmailService : IEmailService
    {
        private static readonly string MessageId = "X-Message-Id";
        private readonly IEmailRepository emailRepository;

        public EmailService(IEmailRepository emailRepository)
        {
            this.emailRepository = emailRepository;
        }
        public async Task<EmailResponse> sendEmail(EmailContract contract)
        {
            string strSendGridApiKey = await emailRepository.getSendGridApiKey();
            string strEmailAddress = await emailRepository.getEmailAddress();
            SendGridClient _client = new SendGridClient(strSendGridApiKey);
            var emailMessage = new SendGridMessage()
            {
                From = new EmailAddress(strEmailAddress, contract.Alias),
                Subject = contract.Subject,
                HtmlContent = contract.Body,

            };
            emailMessage.AddTo(new EmailAddress(contract.ToEmailAddress));
            if (!string.IsNullOrWhiteSpace(contract.BccEmailAddress))
            {
                emailMessage.AddBcc(new EmailAddress(contract.BccEmailAddress));
            }

            if (!string.IsNullOrWhiteSpace(contract.CcEmailAddress))
            {
                emailMessage.AddBcc(new EmailAddress(contract.CcEmailAddress));
            }
            var content = await _client.SendEmailAsync(emailMessage);

            var Email = ProcessResponse(content);
            return Email;
        }
        private EmailResponse ProcessResponse(Response response)
        {
            if (response.StatusCode.Equals(System.Net.HttpStatusCode.Accepted)
                || response.StatusCode.Equals(System.Net.HttpStatusCode.OK))
            {
                return ToMailResponse(response);
            }
            var errorResponse = response.Body.ReadAsStringAsync().Result;
            throw new EmailServiceException(response.StatusCode.ToString(), errorResponse);
        }
        private static EmailResponse ToMailResponse(Response response)
        {
            if (response == null)
                return null;

            var headers = (HttpHeaders)response.Headers;
            string messageId = headers.GetValues(MessageId).FirstOrDefault() ?? "";
            EmailResponse result = new EmailResponse
            {
                UniqueMessageId = messageId,
                DateSent = DateTime.UtcNow
            };
            return result;
        }
        public void SendMail(string SendTo, string Subject, string Body)
        {
            string _sender = SettingData.EMAIL_SENDER;
            string _password = SettingData.EMAIL_PASSWORD;

            SmtpClient client = new SmtpClient(SettingData.EMAIL_SMTP);

            client.Port = 587;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            System.Net.NetworkCredential credentials = new System.Net.NetworkCredential(_sender, _password);
            client.EnableSsl = true;
            client.Credentials = credentials;

            MailMessage message = new MailMessage(_sender, SendTo);
            message.Subject = Subject;
            message.Body = Body;
            message.IsBodyHtml = true;

            client.Send(message);
        }
    }
}

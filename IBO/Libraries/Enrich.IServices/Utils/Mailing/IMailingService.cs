using Enrich.DataTransfer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IServices.Utils.Mailing
{
    public interface IMailingService
    {
        Task<string> Send(string to, string subject, string body, string bcc = "", string cc = "", string fileAttach = null);

        /// <summary>
        /// gui email thong bao
        /// </summary>
        /// <param name="body"></param>
        /// <param name="to"></param>
        /// <param name="fullname"></param>
        /// <param name="subject"></param>
        /// <param name="cc">email can cc</param>
        /// <param name="signature">false: khong dung default footer.</param>
        Task<string> SendEmailNotice_Vi(string body, string to, string firstname, string subject, string cc = "", bool signature = true);

        Task<string> SendEmailNotice(string body, string to, string firstname, string subject, string cc = "", string bcc = "");

        /// <summary>
        ///  Email notice when password has changed
        /// </summary>
        Task SendEmailAfterChangedPass(string memberName, string username, string password, string to);

        /// <summary>
        /// Gui email sau khi reset pass
        /// </summary>
        Task SendEmailAfterResetPass(string memberName, string username, string password, string to);

        /// <summary>
        /// gui email thong bao den nhan vien sau khi tk cua ho duoc tao
        /// </summary>
        Task<string> SendEmailAfterEmployeesCreated(string firstname, string username, string password, string partnerId, string cemail, string cepass, string to, string cc = "", string bcc = "");

        /// <summary>
        /// gui email den IT thong bao tao companyemail cho nhan vien
        /// <paramref name="listEmail">List<Dictionary<string,string>> ["email","pass"]</paramref>
        /// </summary>
        Task<string> SendEmailToITNoticeNewStaff(string toITEmail, string firstName, string lastName, string cellphone, string printName, string cemail, string cepass, string title, string cc = "");

        #region Send Email Using SendGrid
        /// <summary>
        /// Send email
        /// </summary>
        /// <param name="to">cach bang dau ; neu co nhieu hon 1 email</param>
		///<param name="name">cach bang dau ; neu co nhieu hon 1 email</param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="bcc">chuoi cac email can bbc ngan cach boi dau ;</param>
        /// <returns></returns>
        Task<string> SendBySendGrid(string to, string name, string subject, string body, string cc = "", string bcc = "", bool template = true, string fileAttach = "");

        /// <summary>
        /// Send email
        /// </summary>
        /// <param name="to">cach bang dau ; neu co nhieu hon 1 email</param>
        ///<param name="firstName">cach bang dau ; neu co nhieu hon 1 email</param>
        /// <param name="data"></param>
        /// <param name="bcc">chuoi cac email can bbc ngan cach boi dau ;</param>
        /// <param name="templateId"></param>
        /// <returns></returns>
        Task<string> SendBySendGridWithTemplate(string to, string firstName, string templateId, string cc, object data = null, string bcc = "");

        Task<string> SendNotifyResetPassword(string to, string name, string SalonName, string UserId, string UserPassword, string cc, string link = "", string bcc = "");

        Task<string> SendNotifySubmitEstimate(string to, string name, object mailData, string cc = "", string bcc = "");

        /// <summary>
        /// Gửi email khi Installation Complete (active store_license)
        /// </summary>
        /// <param name="to">Email salon</param>
        /// <param name="name">Sender name</param>
        /// <param name="bcc">bcc</param>
        /// <param name="EmailData">Format : SendGridEmailTemplateData.InstallCompleteTemplate</param>
        /// <returns></returns>
        Task<string> SendNotifyInstallationComplete(string to, string name, string bcc, SendGridEmailTemplateData.InstallCompleteTemplate EmailData);
        Task<string> SendBySendGrid_ReceiptPayment(string to, string name, object mailData, string cc = "", string bcc = "");

        Task<string> SendNotifyOutgoingWithTemplate(string to, string firstName, string subject, string cc, object data = null, string fileAttach = "");

        Task<string> SendEmail_HR(string to, string firstName, string subject, string cc, object data = null, string fileAttach = "");

        Task<string> SendEmailRequireGoogleAuth(string email);
        #endregion
    }

}

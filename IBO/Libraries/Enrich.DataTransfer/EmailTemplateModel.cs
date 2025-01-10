using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.DataTransfer
{
    public class SendGridEmailTemplateData
    {
        /// <summary>
        /// thong bao kem link questionare form gui cho khach hang.
        /// </summary>
        public class QuestionareEmailData
        {
            public string client { get; set; }
            public string link { get; set; }
            public string salon_email { get; set; }
            public string salon_password { get; set; }
        }

        /// <summary>
        /// thong bao cho khach hang sau khi hoan tat giao hang.
        /// </summary>
        public class EmailDeplomentTemplate
        {
            public string SalonName { get; set; }
            public string CustomerName { get; set; }
            public string OrderNumber { get; set; }
            public string OrderName { get; set; }
            public string CarrierName { get; set; }
            public string TrackingNumber { get; set; }
            public string DateShipped { get; set; }
            public string Note { get; set; }
            public string Address { get; set; }
            public string Phone { get; set; }
            public string LinkPayment { get; set; }
            public string CreatedOn { get; set; }
            public List<object> services { get; set; }
        }

        /// <summary>
        /// thong bao kem link questionare va link ticket cho nhan vien sau khi khach hang submit questionare form.
        /// </summary>
        public class EmailNoticeQuestionareTemplate
        {
            public string staff_name { get; set; }
            public string salon_name { get; set; }
            public string url_ticket { get; set; }
            public string url_question { get; set; }
            public string update { get; set; }
        }

        /// <summary>
        /// thong bao cho salon khi reset password.
        /// </summary>
        public class ResetPasswordTemplate
        {
            public string salon_name { get; set; }
            public string login_user { get; set; }
            public string login_password { get; set; }

            public Link link { get; set; }
        }
        public class InstallCompleteTemplate
        {
            public string salon_name { get; set; }
            public string login_user { get; set; }
            public string login_password { get; set; }
            public History salon { get; set; }
            public History checkin { get; set; }
            public string pos_link { get; set; }
            public string checkin_link { get; set; }
            public string email_support { get; set; }
            public string phone_suport { get; set; }
        }
        public class InstallCompleteTemplateSlice
        {
            public string salon_name { get; set; }
        }
        public class VerifyRegister
        {
            public string name { get; set; }
            public string number { get; set; }
            public string link { get; set; }
            public string trial { get; set; }
        }
        public class MerchantSubscribe
        {
            public string name { get; set; }
            public string link { get; set; }
            public string phone { get; set; }
            public string email { get; set; }
        }
        public class NotificationAssigned
        {
            public string sales_person { get; set; }
            public string link { get; set; }
            public string sales_lead_name { get; set; }
            public string phone { get; set; }
        }
        public class History
        {
            public List<OrderHistory> orderHistory { get; set; }
        }
        public class OrderHistory
        {
            public string salon_numerical_order { get; set; }
            public string salon_pairing_code { get; set; }
            public string checkin_numerical_order { get; set; }
            public string checkin_pairing_code { get; set; }
        }
        public class Link
        {
            public string LinkChangePass { get; set; }
        }
        public class GiftCardsOrderingForm
        {
            public string owner_name { get; set; }
            public string link { get; set; }
        }
        public class NotifySubmitGiftCard
        {
            public string owner_name { get; set; }
            public string link { get; set; }
            public string sales_person { get; set; }
        }
        public class EmailMerchantForm
        {
            public string subject { get; set; }
            public string contentaddon { get; set; }
            public string salon_name { get; set; }
            public string link { get; set; }
        }
        public class EmailMerchantFormToStaff
        {
            public string staff_name { get; set; }
            public string salon_code { get; set; }
            public string salon_name { get; set; }
            public string link { get; set; }
        }
    }


    public class T_EmailTemplate_customize
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string TemplateGroupName { get; set; }
    }
    public class EmailTemplateContent
    {
        public string MERCHANT_OWNER_NAME { get; set; }
        public string MERCHANT_BUSINESS_NAME { get; set; }
        public string COMPANY_NAME { get; set; }
        public string COMPANY_EMAIL { get; set; }
        public string COMPANY_ADDRESS { get; set; }
        public string YOUR_NAME { get; set; }
        public string YOUR_PHONE { get; set; }
        public string YOUR_EMAIL { get; set; }
    }

}

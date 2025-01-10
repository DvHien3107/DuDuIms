using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Areas.Page.Models.Customize
{
    public class FormModel
    {
        public class BankChangeForm_Data
        {
            public string Date { get; set; }
            public string MID { get; set; }
            public string OwnerName { get; set; }
            public string BusinessName { get; set; }
            public string PhoneNumber { get; set; }
            public string DepositBankName { get; set; }
            public string DepositAccountNumber { get; set; }
            public string DepositRoutingNumber { get; set; }
            public string WithdrawalBankName { get; set; }
            public string WithdrawalAccountNumber { get; set; }
            public string WithdrawalRoutingNumber { get; set; }
            public string BankAccount { get; set; }
            
        }

        public class PaymentForm_Data
        {
            public string MID { get; set; }
            public string OwnerName { get; set; }
            public string MerchantCode { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
            public string Day { get; set; }
            public string Month { get; set; }
            public string Year { get; set; }
            public string BusinessName { get; set; }
            public string BusinessStreetAddress { get; set; }
            public string BusinessCity { get; set; }
            public string BusinessState { get; set; }
            public string BusinessZip { get; set; }
            public string Routing { get; set; }
            public string AccountNumber { get; set; }
            public string BankName { get; set; }

            public long? InvoiceNumber { get; set; }
            public decimal? GrandTotal { get; set; }
            public decimal? ServiceMonthlyFee { get; set; }
        }


        public class Priority_Data
        {
            public string BusinessName { get; set; }
            public string LocationAddress { get; set; }
            public string LCity { get; set; }
            public string LState { get; set; }
            public string LZip { get; set; }
            public string LPhone { get; set; }
            public string LegalName { get; set; }
            public string CorporateAddress { get; set; }
            public string CCity { get; set; }
            public string CState { get; set; }
            public string CZip { get; set; }
            public string ContactName { get; set; }
            public string ContactPhone { get; set; }
            public string BusinessEmail { get; set; }
            public string OwnerName { get; set; }
            public string OwnerAddress { get; set; }
            public string OCity { get; set; }
            public string OState { get; set; }
            public string OZip { get; set; }
            public string SocialSecurity { get; set; }
            public string OEmail { get; set; }
            public string DepositBankName { get; set; }
            public string BankRouting { get; set; }
            public string BankAccount { get; set; }
            public bool ACH_Individual { get; set; }
           
        }


        public class RefundForm_Data
        {
            public string MID { get; set; }
            public string OwnerName { get; set; }
            public string OrderCode { get; set; }
            public string MerchantCode { get; set; }
            public string BusinessName { get; set; }
            public string BusinessStreetAddress { get; set; }
            public string BusinessCity { get; set; }
            public string BusinessState { get; set; }
            public string BusinessZip { get; set; }
            public string RefundCode { get; set; }
            public decimal? RefundAmount { get; set; }
            public string Reason { get; set; }
            public string Subject { get; set; }
            public string Content { get; set; }
        }

        public class NameChangeForm_Data
        {
            //MERCHANT ACCOUNT INFORMATION
            public string Date { get; set; }
            public string MID { get; set; }
            public string OwnerName { get; set; }
            public string BusinessName { get; set; }
            public string LegalName { get; set; }

            //DBA CHANGES REQUESTED
            public bool NewBusinessName_check { get; set; }
            public string NewBusinessName { get; set; }
            public bool LegalAddress_check { get; set; }
            public string LegalAddress { get; set; }
            public bool BusinessAddress_check { get; set; }
            public string BusinessAddress { get; set; }
            public bool EmailAddress_check { get; set; }
            public string EmailAddress { get; set; }
            public bool BusinessPhone_check { get; set; }
            public string BusinessPhone { get; set; }
            public bool Fax_check { get; set; }
            public string Fax { get; set; }
            public bool Website_check { get; set; }
            public string Website { get; set; }

            //PRICING AND CARD TYPE CHANGES REQUESTED
            public bool AmexOPT_check { get; set; }
            public bool Interchange_check { get; set; }
            public bool Tiered_check { get; set; }
            public string AmexOPT_rate { get; set; }
            public bool AmexDirect_check { get; set; }
            public bool AddDiscover_check { get; set; }
            public bool PinDebitDiscount_check { get; set; }
            public string PinDebitDiscount_rate { get; set; }
            public bool EBT_check { get; set; }
            public string FNS { get; set; }
            public string ETBTransFee { get; set; }
            public bool AddCashBenefits_check { get; set; }
            public bool AddMerchantBenefits_check { get; set; }
            public string MerchantBenefits_rate { get; set; }
            public bool VisaMastercardDiscoverDiscount_check { get; set; }
            public string VisaMastercardDiscoverDiscount_rate { get; set; }
            public bool CheckcardDiscount_check { get; set; }
            public string CheckCardDiscount_rate { get; set; }
            public bool Other1_check { get; set; }
            public string Other1 { get; set; }
            public string Other1_rate { get; set; }
            public bool Other2_check { get; set; }
            public string Other2 { get; set; }
            public string Other2_rate { get; set; }
            public string Notes { get; set; }

        }

        //public class CustomizeRequestForm
        //{
        //    public string UId { get; set; }
        //    public int Index { get; set; }
        //    public string Question { get; set; }
        //    public string Type { get; set; }

        //}

    }
}
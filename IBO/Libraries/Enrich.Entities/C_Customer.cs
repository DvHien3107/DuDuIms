using Enrich.Core.UnitOfWork.Data;
using System;
using System.Collections.Generic;

namespace Enrich.Entities
{    
    public partial class C_Customer:BaseEntity
    {
        public long Id { get; set; }
        public string CustomerCode { get; set; }
        public string ContactName { get; set; }
        public string ContactTitle { get; set; }
        public Nullable<System.DateTime> Birthday { get; set; }
        public string Email { get; set; }
        public string SocialSecurity { get; set; }
        public string CellPhone { get; set; }
        public string Password { get; set; }
        public string CompanyName { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Zipcode { get; set; }
        public string Country { get; set; }
        public string DriverLicense { get; set; }
        public string CompanyEmail { get; set; }
        public string Fax { get; set; }
        public string Comment { get; set; }
        public string Type { get; set; }
        public Nullable<decimal> Percentage { get; set; }
        public string CreateBy { get; set; }
        public Nullable<System.DateTime> CreateAt { get; set; }
        public string UpdateBy { get; set; }
        public Nullable<int> Active { get; set; }
        public Nullable<int> StarNumber { get; set; }
        public string Gender { get; set; }
        public string BusinessName { get; set; }
        public string BusinessPhone { get; set; }
        public string BusinessEmail { get; set; }
        public string BusinessState { get; set; }
        public string BusinessCity { get; set; }
        public string BusinessZipCode { get; set; }
        public string BusinessCountry { get; set; }
        public string Website { get; set; }
        public string PreferredLanguage { get; set; }
        public string LegalName { get; set; }
        public string OwnerMobile { get; set; }
        public string OwnerSocial { get; set; }
        public string ContactAddress { get; set; }
        public string BusinessAddressCivicNumber { get; set; }
        public string BusinessAddressStreet { get; set; }
        public string FederalTaxId { get; set; }
        public Nullable<System.DateTime> BusinessStartDate { get; set; }
        public string OwnerName { get; set; }
        public string BusinessAddress { get; set; }
        public string MerchantChain { get; set; }
        public string OwnerAddressCivicNumber { get; set; }
        public string OwnerAddressStreet { get; set; }
        public string WordDetermine { get; set; }
        public string BusinessDescription { get; set; }
        public string CurrentProcessorName { get; set; }
        public string OwnersOrOfficicers_PreviousProcessingIndicator { get; set; }
        public string BankAccountHolderName { get; set; }
        public string BankAccountType { get; set; }
        public Nullable<bool> CreditCardSettltment { get; set; }
        public Nullable<bool> DebitSettltment { get; set; }
        public string StoreCode { get; set; }
        public Nullable<bool> StoreStatus { get; set; }
        public string SalonAddress1 { get; set; }
        public string SalonAddress2 { get; set; }
        public string SalonCity { get; set; }
        public string SalonState { get; set; }
        public string SalonZipcode { get; set; }
        public string SalonPhone { get; set; }
        public string SalonTimeZone { get; set; }
        public string SalonEmail { get; set; }
        public string MD5PassWord { get; set; }
        public string RequirePassChange { get; set; }
        public string Version { get; set; }
        public string SalonTimeZone_Number { get; set; }
        public string More_Info { get; set; }
        public string MxMerchant_Id { get; set; }
        public string DepositAccountNumber { get; set; }
        public string DepositBankName { get; set; }
        public string DepositRoutingNumber { get; set; }
        public string WithdrawalAccountNumber { get; set; }
        public string WithdrawalBankName { get; set; }
        public string WithdrawalRoutingNumber { get; set; }
        public Nullable<bool> ActiveRecuring { get; set; }
        public Nullable<bool> IsSendQuestionare { get; set; }
        public string MangoEmail { get; set; }
        public string PreferredName { get; set; }
        public string PartnerCode { get; set; }
        public string PartnerName { get; set; }
        public string MemberNumber { get; set; }
        public string FullName { get; set; }
    }
}

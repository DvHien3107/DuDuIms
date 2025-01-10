using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel
{
    public class Merchant_IndexView
    {
        public C_Customer Customer { get; set; }
        public Store_Services License { get; set; }
        public string AddressLine { get; set; }
        public int? Remaning { get; set; }
        public int? LifeTime { get; set; }
        public DateTime? RenewDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string LicenseName { get; set; }
    }
    public class CustomerMerchantViewModel
    {
        //Customer info
        public long CustomerId { get; set; }
        public string CustomerCode { get; set; }
        public string CompanyName { get; set; }
        public string ContactName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPassword { get; set; }
        public string ContactTitle { get; set; }
        public Nullable<System.DateTime> Birthday { get; set; }
        public string SocialSecurity { get; set; }
        public string CellPhone { get; set; }
        public string Address { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Zipcode { get; set; }
        public string Country { get; set; }
        public string DriverLicense { get; set; }
        public Nullable<decimal> Percentage { get; set; }
        public string BankName { get; set; }
        public string BankRoutig { get; set; }
        public string BankDDA { get; set; }
        public string UpdateHistory { get; set; }

        //Business info
        public string BusinessName { get; set; }
        public string BusinessPhone { get; set; }
        public string BusinessEmail { get; set; }
        public string BusinessAddress { get; set; }
        public string BusinessState { get; set; }
        public string BusinessCity { get; set; }
        public string BusinessZipCode { get; set; }
        public string BusinessCountry { get; set; }

        //Merchant info
        public long MerchantId { get; set; }

        public string Phone { get; set; }
        public string Email { get; set; }
        public string Processor { get; set; }
        public string M_ID { get; set; }
        public string ContactPerson { get; set; }
        public Nullable<bool> S_Active { get; set; }
        public string LegalName { get; set; }
        public string PreferredContact { get; set; }
        public Nullable<System.DateTime> S_ApprovalDate { get; set; }
        public Nullable<System.DateTime> S_ActiveDate { get; set; }
        public string FederalTaxId { get; set; }
        public string PreferredLanguage { get; set; }
        public Nullable<System.DateTime> S_LastBatchDate { get; set; }
        public Nullable<System.DateTime> S_ClosesDate { get; set; }
        public string BusinessFormation { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string A_Address { get; set; }
        public string A_State { get; set; }
        public string A_City { get; set; }
        public string A_ZipCode { get; set; }
        public string A_Country { get; set; }
        public string CreditCardNumber { get; set; }
        public string SalesAgent { get; set; }
        public string ChainRelationship { get; set; }
        public string MerchantChain { get; set; }
        public Nullable<bool> S_FreeSupply { get; set; }
        public string Website { get; set; }
        public Nullable<bool> S_EMV { get; set; }
        public Nullable<bool> S_PremiumMerchant { get; set; }
        public string RemarksBoarding { get; set; }
        public string RemarksTech { get; set; }
        public string RegionalManager { get; set; }
        public Nullable<bool> S_EvieRewards { get; set; }
    }
    public class Devices_in_Order
    {
        public O_Orders Order { get; set; }
        public List<Order_Products> Devices { get; set; }
    }
    public class C_Customer_select_view
    {
        public long Id { get; set; }
        public string OwnerName { get; set; }
        public string BusinessName { get; set; }
        public string BusinessPhone { get; set; }
        public string CellPhone { get; set; }
        public string ApplicationId { get; set; }
        public string MerChantId { get; set; }
        public string ProccessorName { get; set; }
        public int CompletedStep { get; set; }
        public string Address { get; set; }
        public string OwnerEmail { get; set; }
        public string MerchantStatus { get; set; }
        public long? TicketId { get; set; }
        public long SubscribeId { get; internal set; }
        public string MerChantCode { get; internal set; }
    }
}
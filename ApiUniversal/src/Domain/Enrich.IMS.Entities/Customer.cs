using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Enrich.IMS.Entities
{
    /// <summary>
    /// It is merchant
    /// </summary>
    public partial class Customer
    {
        #region main infor

        /// <summary>
        /// Id
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }

        /// <summary>
        /// Customer code
        /// </summary>
        public string CustomerCode { get; set; }

        /// <summary>
        /// Create by
        /// </summary>
        public string CreateBy { get; set; }

        /// <summary>
        /// Create at
        /// </summary>
        public DateTime? CreateAt { get; set; }

        /// <summary>
        /// Updat by
        /// </summary>
        public string UpdateBy { get; set; }

        /// <summary>
        /// Store code
        /// </summary>
        public string StoreCode { get; set; }

        /// <summary>
        /// STORE_OF_MERCHANT
        /// STORE_IN_HOUSE
        /// null = STORE_OF_MERCHANT
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// To determine what customer is. For now, it is trial and Slice
        /// Trial: trial
        /// Slice: long life
        /// </summary>
        public string WordDetermine { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public long? MxMerchantId { get; set; }


        #endregion

        #region Salon Infomation

        /// <summary>
        /// Account manager
        /// </summary>
        public string MemberNumber { get; set; }

        /// <summary>
        /// Account manager name (Dont use in this version)
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// salon name
        /// </summary>
        public string BusinessName { get; set; }

        /// <summary>
        /// Partner code
        /// </summary>
        public string PartnerCode { get; set; }

        /// <summary>
        /// Partner Name (Dont use in this version)
        /// </summary>
        public string PartnerName { get; set; }

        /// <summary>
        /// Salon email
        /// </summary>
        public string SalonEmail { get; set; }

        /// <summary>
        /// Address 1 (main address)
        /// </summary>
        public string SalonAddress1 { get; set; }

        /// <summary>
        /// Address 2 (suite #)
        /// </summary>
        public string SalonAddress2 { get; set; }

        /// <summary>
        /// Salon city
        /// </summary>
        public string SalonCity { get; set; }

        /// <summary>
        /// Salon state
        public string SalonState { get; set; }

        /// <summary>
        /// Salon zipcode
        /// </summary>
        public string SalonZipcode { get; set; }

        /// <summary>
        /// Salon phone
        /// </summary>
        public string SalonPhone { get; set; }

        /// <summary>
        /// Salon timezone. Exp: Eastern
        /// </summary>
        public string SalonTimeZone { get; set; }

        /// <summary>
        /// Salon TimeZone number. Exp: -5
        /// </summary>
        public string SalonTimeZoneNumber { get; set; }

        /// <summary>
        /// Federal Tax Id 
        /// </summary>
        public string FederalTaxId { get; set; }

        /// <summary>
        /// Start up date 
        /// </summary>
        public DateTime? BusinessStartDate { get; set; }

        /// <summary>
        /// Go-live Date
        /// </summary>
        public DateTime? GoLiveDate { get; set; }

        /// <summary>
        /// Salon note
        /// </summary>
        public string BusinessDescription { get; set; }

        #endregion

        #region Owner

        /// <summary>
        /// Owner/Docusign name
        /// </summary>
        public string OwnerName { get; set; }

        /// <summary>
        /// Owner/Docusign email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Owner phone 
        /// </summary>
        public string CellPhone { get; set; }

        public string OwnerSocial { get; set; }

        /// <summary>
        /// Owner city 
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Owner address 2
        /// </summary>
        public string OwnerAddressCivicNumber { get; set; }

        /// <summary>
        /// Owner address 1
        /// </summary>
        public string OwnerAddressStreet { get; set; }

        /// <summary>
        /// Owner state
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Owner zipcode
        /// </summary>
        public string Zipcode { get; set; }

        /// <summary>
        /// Owner country
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Social number
        /// </summary>
        public string SocialSecurity { get; set; }

        /// <summary>
        /// Birthday
        /// </summary>
        public DateTime? Birthday { get; set; }

        /// <summary>
        /// Preferred language
        /// </summary>
        public string PreferredLanguage { get; set; }

        /// <summary>
        /// Preferred Name
        /// </summary>
        public string PreferredName { get; set; }

        #endregion

        #region POS

        /// <summary>
        /// Mango email (using to login in mango)
        /// </summary>
        public string MangoEmail { get; set; }

        /// <summary>
        /// Password  (using to login in mango)
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Password after hash by md5  (using to login in mango)
        /// </summary>
        public string MD5PassWord { get; set; }

        /// <summary>
        /// Require password change  (using for mango)
        /// </summary>
        public string RequirePassChange { get; set; }

        #endregion

        #region Bank Infomation ACH

        /// <summary>
        /// Bank name
        /// </summary>
        public string DepositBankName { get; set; }

        /// <summary>
        /// Bank DDA/Account Number 
        /// </summary>
        public string DepositAccountNumber { get; set; }

        /// <summary>
        /// Bank transit
        ///  </summary>
        public string DepositRoutingNumber { get; set; }

        #endregion

        /// <summary>
        /// use CellPhone 
        /// </summary>
        public string OwnerMobile { get; set; }

        /// <summary>
        /// use OwnerName
        /// </summary>
        public string ContactName { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        public string ContactTitle { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        public string CompanyName { get; set; }

        // dont use
        public string DriverLicense { get; set; }

        /// <summary>
        ///  dont use
        /// </summary>
        public string CompanyEmail { get; set; }

        /// <summary>
        ///  dont use
        /// </summary>
        public string Fax { get; set; }

        /// <summary>
        ///  dont use
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        ///  dont use
        /// </summary>
        public decimal? Percentage { get; set; }


        /// <summary>
        /// dont use
        /// </summary>
        public bool? StoreStatus { get; set; }

        /// <summary>
        ///  dont use
        /// </summary>
        public int? StarNumber { get; set; }

        /// <summary>
        ///  dont use
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// use SalonPhone
        /// </summary>
        public string BusinessPhone { get; set; }

        /// <summary>
        /// use SalonEmail
        /// </summary>
        public string BusinessEmail { get; set; }

        /// <summary>
        /// use Salon state
        /// </summary>
        public string BusinessState { get; set; }

        /// <summary>
        /// use SalonCity
        /// </summary>
        public string BusinessCity { get; set; }

        /// <summary>
        /// use SalonZipCode
        /// </summary>
        public string BusinessZipCode { get; set; }

        /// <summary>
        /// use SalonCountry
        /// </summary>
        public string BusinessCountry { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        public string Website { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        public string LegalName { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        public string ContactAddress { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        public string BusinessAddressCivicNumber { get; set; }

        /// <summary>
        ///  use SalonAddress1
        /// </summary>
        public string BusinessAddressStreet { get; set; }

        /// <summary>
        /// use SalonAddress1
        /// </summary>
        public string BusinessAddress { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        public string MerchantChain { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        public string CurrentProcessorName { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        public string BankAccountHolderName { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        public string BankAccountType { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        public bool? CreditCardSettltment { get; set; }

        /// <summary>
        ///  dont use
        /// </summary>
        public bool? DebitSettltment { get; set; }

        /// <summary>
        ///  dont use
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        public string MoreInfo { get; set; }

    
        /// <summary>
        /// need check again
        ///  </summary>
        public string WithdrawalAccountNumber { get; set; }

        /// <summary>
        /// need check again
        ///  </summary>
        public string WithdrawalBankName { get; set; }

        /// <summary>
        /// need check again
        ///  </summary>
        public string WithdrawalRoutingNumber { get; set; }

        /// <summary>
        /// dont use
        ///  </summary>
        public bool? ActiveRecuring { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        public bool? IsSendQuestionare { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        public string OwnersOrOfficicers_PreviousProcessingIndicator { get; set; }

        /// <summary>
        /// dont use. Use store service to determine it
        /// </summary>
        public int? Active { get; set; } = 0;

        /// <summary>
        /// cancel date
        /// </summary>
        public DateTime? CancelDate { get; set; }

        public string AddressLine()
        {
            var address = SalonAddressLine();
            if (string.IsNullOrEmpty(address)) address = BusinessAddressLine();
            if (string.IsNullOrEmpty(address)) address = DefaultsAddressLine();
            return address;
        }

        private string DefaultsAddressLine()
        {
            return string.Join(", ", new string[] {
                BusinessAddressStreet?.Replace(",", "") ?? " ", City?.Replace(",", "") ?? " ", State?.Replace(",", "") ?? " ", (Zipcode?.Replace(",", "") ?? " ") + " " + (Country?.Replace(",", "") ?? BusinessCountry?.Replace(",", "") ?? "United States")
            }.Where(s => !string.IsNullOrEmpty(s)));
        }

        public string SalonAddressLine()
        {
            if (string.IsNullOrEmpty(SalonAddress1)) return "";
            return string.Join(", ", new string[] {
                SalonAddress1?.Replace(",", "") ?? " " + SalonAddress2?.Replace(",", "") ?? " ", SalonCity?.Replace(",", "") ?? " ", SalonState?.Replace(",", "") ?? " ", (SalonZipcode?.Replace(",", "") ?? " ") + " " + (BusinessCountry?.Replace(",", "") ?? "United States")
            }.Where(s => !string.IsNullOrEmpty(s)));
        }

        public string BusinessAddressLine()
        {
            if (string.IsNullOrEmpty(BusinessAddressStreet)) return "";
            return string.Join(", ", new string[] {
                BusinessAddressStreet?.Replace(",", "") ?? " ", BusinessCity?.Replace(",", "") ?? " ", BusinessState?.Replace(",", "") ?? " ", (BusinessZipCode?.Replace(",", "") ?? " ") + " " + (BusinessCountry?.Replace(",", "") ?? "United States")
            }.Where(s => !string.IsNullOrEmpty(s)));
        }

        public string GetSalonAddress()
        {
            return SalonAddress1 ?? BusinessAddressStreet ?? OwnerAddressStreet;
        }

        public string GetSalonCity()
        {
            return SalonCity ?? BusinessCity ?? City;
        }

        public string GetSalonState()
        {
            return SalonState ?? BusinessState ?? State;
        }

        public string GetSalonZipCode()
        {
            return $"{SalonZipcode ?? BusinessZipCode ?? Zipcode} {BusinessCountry ?? Country ?? "United States"}";
        }
    }
}

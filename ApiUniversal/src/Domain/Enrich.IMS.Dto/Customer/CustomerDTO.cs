using Enrich.Dto.Base.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto
{
    [FieldDb(table: $"{SqlTables.Customer}")]
    public class CustomerDto
    {
        #region main infor

        /// <summary>
        /// Id
        /// </summary>       
        [FieldDb(nameof(Id))]
        public long Id { get; set; }

        /// <summary>
        /// Customer code
        /// </summary>
        [FieldDb(nameof(CustomerCode))]
        public string CustomerCode { get; set; }

        /// <summary>
        /// Create by
        /// </summary>
        [FieldDb(nameof(CreateBy))]
        public string CreateBy { get; set; }

        /// <summary>
        /// Create at
        /// </summary>
        [FieldDb(nameof(CreateAt))]
        public DateTime? CreateAt { get; set; }

        /// <summary>
        /// Updat by
        /// </summary>
        [FieldDb(nameof(UpdateBy))]
        public string UpdateBy { get; set; }

        /// <summary>
        /// Store code
        /// </summary>
        [FieldDb(nameof(StoreCode))]
        public string StoreCode { get; set; }

        /// <summary>
        /// STORE_OF_MERCHANT
        /// STORE_IN_HOUSE
        /// null = STORE_OF_MERCHANT
        /// </summary>
        [FieldDb(nameof(Type))]
        public string Type { get; set; }

        /// <summary>
        /// To determine what customer is. For now, it is trial and Slice
        /// Trial: trial
        /// Slice: long life
        /// </summary>        
        [FieldDb(nameof(WordDetermine))]
        public string WordDetermine { get; set; }

        /// <summary>
        /// MxMerchant Id
        /// </summary>
        [FieldDb($"{SqlColumns.Customer.MxMerchantId}")]
        public long? MxMerchantId { get; set; }


        #endregion

        #region Salon Infomation

        /// <summary>
        /// Account manager
        /// </summary>
        [FieldDb(nameof(MemberNumber))]
        public string MemberNumber { get; set; }

        /// <summary>
        /// Account manager name (Dont use in this version)
        /// </summary>
        [FieldDb(nameof(FullName))]
        public string FullName { get; set; }

        /// <summary>
        /// salon name
        /// </summary>
        [FieldDb(nameof(BusinessName))]
        public string BusinessName { get; set; }

        /// <summary>
        /// Partner code
        /// </summary>
        [FieldDb(nameof(PartnerCode))]
        public string PartnerCode { get; set; }

        /// <summary>
        /// Partner Name (Dont use in this version)
        /// </summary>
        [FieldDb(nameof(PartnerName))]
        public string PartnerName { get; set; }

        /// <summary>
        /// Salon email
        /// </summary>
        [FieldDb(nameof(SalonEmail))]
        public string SalonEmail { get; set; }

        /// <summary>
        /// Address 1 (main address)
        /// </summary>
        [FieldDb(nameof(SalonAddress1))]
        public string SalonAddress1 { get; set; }

        /// <summary>
        /// Address 2 (suite #)
        /// </summary>
        [FieldDb(nameof(SalonAddress2))]
        public string SalonAddress2 { get; set; }

        /// <summary>
        /// Salon city
        /// </summary>
        [FieldDb(nameof(SalonCity))]
        public string SalonCity { get; set; }

        /// <summary>
        /// Salon state
        [FieldDb(nameof(SalonState))]
        public string SalonState { get; set; }

        /// <summary>
        /// Salon zipcode
        /// </summary>
        [FieldDb(nameof(SalonZipcode))]
        public string SalonZipcode { get; set; }

        /// <summary>
        /// Salon phone
        /// </summary>
        [FieldDb(nameof(SalonPhone))]
        public string SalonPhone { get; set; }

        /// <summary>
        /// Salon timezone. Exp: Eastern
        /// </summary>
        [FieldDb(nameof(SalonTimeZone))]
        public string SalonTimeZone { get; set; }

        /// <summary>
        /// Salon TimeZone number. Exp: -5
        /// </summary>
        [FieldDb(nameof(SalonTimeZoneNumber))]
        public string SalonTimeZoneNumber { get; set; }

        /// <summary>
        /// Federal Tax Id 
        /// </summary>
        [FieldDb(nameof(FederalTaxId))]
        public string FederalTaxId { get; set; }

        /// <summary>
        /// Start up date 
        /// </summary>
        [FieldDb(nameof(BusinessStartDate))]
        public DateTime? BusinessStartDate { get; set; }

        /// <summary>
        /// Go-live Date
        /// </summary>
        [FieldDb(nameof(GoLiveDate))]
        public DateTime? GoLiveDate { get; set; }

        /// <summary>
        /// Salon note
        /// </summary>
        [FieldDb(nameof(BusinessDescription))]
        public string BusinessDescription { get; set; }

        #endregion

        #region Owner

        /// <summary>
        /// Owner/Docusign name
        /// </summary>
        [FieldDb(nameof(OwnerName))]
        public string OwnerName { get; set; }

        /// <summary>
        /// Owner/Docusign email
        /// </summary>
        [FieldDb(nameof(Email))]
        public string Email { get; set; }

        /// <summary>
        /// Owner phone 
        /// </summary>
        [FieldDb(nameof(CellPhone))]
        public string CellPhone { get; set; }


        /// <summary>
        ///  dont use
        /// </summary>
        [FieldDb(nameof(OwnerSocial))]
        public string OwnerSocial { get; set; }

        /// <summary>
        /// Owner city 
        /// </summary>
        [FieldDb(nameof(City))]
        public string City { get; set; }

        /// <summary>
        /// Owner address 2
        /// </summary>
        [FieldDb(nameof(OwnerAddressCivicNumber))]
        public string OwnerAddressCivicNumber { get; set; }

        /// <summary>
        /// Owner address 1
        /// </summary>
        [FieldDb(nameof(OwnerAddressStreet))]
        public string OwnerAddressStreet { get; set; }

        /// <summary>
        /// Owner state
        /// </summary>
        [FieldDb(nameof(State))]
        public string State { get; set; }

        /// <summary>
        /// Owner zipcode
        /// </summary>
        [FieldDb(nameof(Zipcode))]
        public string Zipcode { get; set; }

        /// <summary>
        /// Owner country
        /// </summary>
        [FieldDb(nameof(Country))]
        public string Country { get; set; }

        /// <summary>
        /// Social number
        /// </summary>
        [FieldDb(nameof(SocialSecurity))]
        public string SocialSecurity { get; set; }

        /// <summary>
        /// Birthday
        /// </summary>
        [FieldDb(nameof(Birthday))]
        public DateTime? Birthday { get; set; }

        /// <summary>
        /// Preferred language
        /// </summary>
        [FieldDb(nameof(PreferredLanguage))]
        public string PreferredLanguage { get; set; }

        /// <summary>
        /// Preferred Name
        /// </summary>
        [FieldDb(nameof(PreferredName))]
        public string PreferredName { get; set; }

        #endregion

        #region POS

        /// <summary>
        /// Mango email (using to login in mango)
        /// </summary>
        [FieldDb(nameof(MangoEmail))]
        public string MangoEmail { get; set; }

        /// <summary>
        /// Password  (using to login in mango)
        /// </summary>
        [FieldDb(nameof(Password))]
        public string Password { get; set; }

        /// <summary>
        /// Password after hash by md5  (using to login in mango)
        /// </summary>
        [FieldDb(nameof(MD5PassWord))]
        public string MD5PassWord { get; set; }

        /// <summary>
        /// Require password change  (using for mango)
        /// </summary>
        [FieldDb(nameof(RequirePassChange))]
        public string RequirePassChange { get; set; }

        #endregion

        #region Bank Infomation ACH

        /// <summary>
        /// Bank name
        /// </summary>
        [FieldDb(nameof(DepositBankName))]
        public string DepositBankName { get; set; }

        /// <summary>
        /// Bank DDA/Account Number 
        /// </summary>
        [FieldDb(nameof(DepositAccountNumber))]
        public string DepositAccountNumber { get; set; }

        /// <summary>
        /// Bank transit
        ///  </summary>
        [FieldDb(nameof(DepositRoutingNumber))]
        public string DepositRoutingNumber { get; set; }

        #endregion

        /// <summary>
        /// use CellPhone 
        /// </summary>
        [FieldDb(nameof(OwnerMobile))]
        public string OwnerMobile { get; set; }

        /// <summary>
        /// use OwnerName
        /// </summary>
        [FieldDb(nameof(ContactName))]
        public string ContactName { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        [FieldDb(nameof(ContactTitle))]
        public string ContactTitle { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        [FieldDb(nameof(CompanyName))]
        public string CompanyName { get; set; }

        // dont use
        [FieldDb(nameof(DriverLicense))]
        public string DriverLicense { get; set; }

        /// <summary>
        ///  dont use
        /// </summary>
        [FieldDb(nameof(CompanyEmail))]
        public string CompanyEmail { get; set; }

        /// <summary>
        ///  dont use
        /// </summary>
        [FieldDb(nameof(Fax))]
        public string Fax { get; set; }

        /// <summary>
        ///  dont use
        /// </summary>
        [FieldDb(nameof(Comment))]
        public string Comment { get; set; }

        /// <summary>
        ///  dont use
        /// </summary>
        [FieldDb(nameof(Percentage))]
        public decimal? Percentage { get; set; }


        /// <summary>
        /// dont use
        /// </summary>
        [FieldDb(nameof(StoreStatus))]
        public bool? StoreStatus { get; set; }

        /// <summary>
        ///  dont use
        /// </summary>
        [FieldDb(nameof(StarNumber))]
        public int? StarNumber { get; set; }

        /// <summary>
        ///  dont use
        /// </summary>
        [FieldDb(nameof(Gender))]
        public string Gender { get; set; }

        /// <summary>
        /// use SalonPhone
        /// </summary>
        [FieldDb(nameof(BusinessPhone))]
        public string BusinessPhone { get; set; }

        /// <summary>
        /// use SalonEmail
        /// </summary>
        [FieldDb(nameof(BusinessEmail))]
        public string BusinessEmail { get; set; }

        /// <summary>
        /// use Salon state
        /// </summary>
        [FieldDb(nameof(BusinessState))]
        public string BusinessState { get; set; }

        /// <summary>
        /// use SalonCity
        /// </summary>
        [FieldDb(nameof(BusinessCity))]
        public string BusinessCity { get; set; }

        /// <summary>
        /// use SalonZipCode
        /// </summary>
        [FieldDb(nameof(BusinessZipCode))]
        public string BusinessZipCode { get; set; }

        /// <summary>
        /// use SalonCountry
        /// </summary>
        [FieldDb(nameof(BusinessCountry))]
        public string BusinessCountry { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        [FieldDb(nameof(Website))]
        public string Website { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        [FieldDb(nameof(LegalName))]
        public string LegalName { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        [FieldDb(nameof(ContactAddress))]
        public string ContactAddress { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        [FieldDb(nameof(BusinessAddressCivicNumber))]
        public string BusinessAddressCivicNumber { get; set; }

        /// <summary>
        ///  use SalonAddress1
        /// </summary>
        [FieldDb(nameof(BusinessAddressStreet))]
        public string BusinessAddressStreet { get; set; }

        /// <summary>
        /// use SalonAddress1
        /// </summary>
        [FieldDb(nameof(BusinessAddress))]
        public string BusinessAddress { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        [FieldDb(nameof(MerchantChain))]
        public string MerchantChain { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        [FieldDb(nameof(CurrentProcessorName))]
        public string CurrentProcessorName { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        [FieldDb(nameof(BankAccountHolderName))]
        public string BankAccountHolderName { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        [FieldDb(nameof(BankAccountType))]
        public string BankAccountType { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        [FieldDb(nameof(CreditCardSettltment))]
        public bool? CreditCardSettltment { get; set; }

        /// <summary>
        ///  dont use
        /// </summary>
        [FieldDb(nameof(DebitSettltment))]
        public bool? DebitSettltment { get; set; }

        /// <summary>
        ///  dont use
        /// </summary>
        [FieldDb(nameof(Version))]
        public string Version { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        [FieldDb($"{SqlColumns.Customer.MoreInfo}")]
        public string MoreInfo { get; set; }


        /// <summary>
        /// need check again
        ///  </summary>
        [FieldDb(nameof(WithdrawalAccountNumber))]
        public string WithdrawalAccountNumber { get; set; }

        /// <summary>
        /// need check again
        ///  </summary>
        [FieldDb(nameof(WithdrawalBankName))]
        public string WithdrawalBankName { get; set; }

        /// <summary>
        /// need check again
        ///  </summary>
        [FieldDb(nameof(WithdrawalRoutingNumber))]
        public string WithdrawalRoutingNumber { get; set; }

        /// <summary>
        /// dont use
        ///  </summary>
        [FieldDb(nameof(ActiveRecuring))]
        public bool? ActiveRecuring { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        [FieldDb(nameof(OwnerSocial))]
        public bool? IsSendQuestionare { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        [FieldDb(nameof(OwnersOrOfficicers_PreviousProcessingIndicator))]
        public string OwnersOrOfficicers_PreviousProcessingIndicator { get; set; }

        /// <summary>
        /// dont use. Use store service to determine it
        /// </summary>
        [FieldDb(nameof(Active))]
        public int? Active { get; set; } = 0;

    }
}

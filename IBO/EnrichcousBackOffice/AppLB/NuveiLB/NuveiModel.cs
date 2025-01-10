using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.AppLB.NuveiLB
{
    public class NuveiPayload
    {
        public MerchantBusinessInformation MerchantBusinessInformation { get; set; }
        public DbaInformation DbaInformation { get; set; }
        public OwnersOrOfficers OwnersOrOfficers { get; set; }
        public ContractVersionEtf ContractVersionEtf { get; set; }
        public BankInformation BankInformation { get; set; }
        public string Agent { get; set; }
        public string Office { get; set; }
        //them vao theo file test payload.txt  ma nuvei gui trong team.
        public string User { get; set; }//username

    }
    public class MerchantBusinessInformation
    {
        public string LegalName { get; set; }
        public string CorporateAddressCivicNum { get; set; }
        public string CorporateAddressStreet { get; set; }
        public string CorporateAddressPobox { get; set; }
        public string CorporateAddressUnitDesignator { get; set; }
        public string CorporateAddressUnit { get; set; }
        public string CorporateCity { get; set; }
        public string CorporateState { get; set; }
        public string CorporateZip { get; set; }
        public string CorporateTelephone { get; set; }
        public string FederalTaxId { get; set; }
        public string BusinessEmail { get; set; }
        public string BusinessPresence { get; set; }
        public string BusinessPresenceMonths { get; set; }
        public string MailingAddress { get; set; }
        public string SupportLine { get; set; }
        public string StatementDescriptor { get; set; }
        public string MailingAttention { get; set; }
        public string PrefContactEmail { get; set; }
        public string PrefContactPhone { get; set; }
        //them vao theo file test payload.txt  ma nuvei gui trong team.
        public string OwnershipType { get; set; }//Corporation
        public string GoodsType { get; set; }//Test Items
        public string BusinessDescription { get; set; }
        public string CurrentProcessorName { get; set; }
    }
    public class DbaInformation
    {
        public string DbaName { get; set; }
        public string LocationAddressCivicNum { get; set; }
        public string LocationAddressStreet { get; set; }
        public string LocationAddressUnitDesignator { get; set; }
        public string LocationAddressUnit { get; set; }
        public string LocationCity { get; set; }
        public string LocationState { get; set; }
        public string LocationZip { get; set; }
        public string LocationTelephone { get; set; }
    }
    public class OwnersOrOfficers
    {
        public string OwnerCitizenship { get; set; }
        public string OwnerCountry { get; set; }
        public string BusinessStartupDate { get; set; }
        public List<Owner> OwnerList { get; set; }
        //them vao theo file test payload.txt  ma nuvei gui trong team.
        public string PreviousProcessingIndicator { get; set; }
    }
    public class Owner
    {
        public bool Guarantor { get; set; }
        public string Title { get; set; }
        public string Email { get; set; }
        public int PercentOwnership { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Birthday { get; set; }
        public string ResidenceAddressCivicNum { get; set; }
        public string ResidenceAddressStreet { get; set; }
        public string ResidenceAddressUnitDesignator { get; set; }
        public string ResidenceAddressUnit { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Telephone { get; set; }
        public string SocialSecurity { get; set; }

    }
    public class ContractVersionEtf
    {
        public string MerchantSignDate { get; set; }
    }
    public class BankInformation
    {
        public string BankTransit { get; set; }
        public string BankDda { get; set; }
        //them vao theo file test payload.txt  ma nuvei gui trong team.
        public string BankAccount { get; set; }
    }
}
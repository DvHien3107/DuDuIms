namespace Enrich.IMS.Entities
{
    public partial class MemberSubscription
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PersonalEmail { get; set; }
        public string CellPhone { get; set; }
        public string Password { get; set; }
        public System.DateTime? Birthday { get; set; }
        public string DriverLicense { get; set; }
        public string SocialSecurity { get; set; }
        public string Address { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }
        public string ReferedByNumber { get; set; }
        public string ReferedByName { get; set; }
        public string MemberType { get; set; }
        public string MemberTypeName { get; set; }
        public System.DateTime? CreateAt { get; set; }
        public string ConfirmBy { get; set; }
        public System.DateTime? ConfirmAt { get; set; }
        public string MemberNumber { get; set; }
    }
}

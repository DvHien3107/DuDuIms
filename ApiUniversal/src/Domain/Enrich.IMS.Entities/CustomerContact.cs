
namespace Enrich.IMS.Entities
{
    public partial class CustomerContact
    {
        public long Id { get; set; }
        public long? CustomerId { get; set; }
        public string Name { get; set; }
        public string Authorization { get; set; }
        public string PreferredLanguage { get; set; }
        public string PreferredContact { get; set; }
        public string PhoneNumber { get; set; }
        public string Relationship { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
    }
}

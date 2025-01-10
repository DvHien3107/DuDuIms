using Enrich.Dto.Base.Attributes;

namespace Enrich.IMS.Dto
{   
    public partial class CustomerContactDto
    {
        public long Id { get; set; }
        /// <summary>
        /// Customer Id
        /// </summary>
        [FieldDb(nameof(CustomerId))]
        public long CustomerId { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        [FieldDb(nameof(Name))]
        public string Name { get; set; }

        /// <summary>
        /// Authorization
        /// </summary>
        [FieldDb(nameof(Authorization))]
        public string Authorization { get; set; }

        /// <summary>
        /// Other Notes
        /// </summary>
        [FieldDb(nameof(PreferredLanguage))]
        public string PreferredLanguage { get; set; }

        /// <summary>
        /// PreferredContact
        /// </summary>
        [FieldDb(nameof(PhoneNumber))]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// PreferredContact
        /// </summary>
        [FieldDb(nameof(Relationship))]
        public string Relationship { get; set; }

        /// <summary>
        /// PreferredContact
        /// </summary>
        [FieldDb(nameof(PreferredContact))]
        public string PreferredContact { get; set; }

        /// <summary>
        /// PreferredContact
        /// </summary>
        [FieldDb(nameof(Email))]
        public string Email { get; set; }

        /// <summary>
        /// PreferredContact
        /// </summary>
        [FieldDb(nameof(Address))]
        public string Address { get; set; }
    }
}
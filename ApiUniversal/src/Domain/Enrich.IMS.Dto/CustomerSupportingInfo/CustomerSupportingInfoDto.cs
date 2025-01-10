using Enrich.Dto.Base.Attributes;

namespace Enrich.IMS.Dto
{   
    public partial class CustomerSupportingInfoDto
    {
        public long Id { get; set; }
        /// <summary>
        /// Customer Id
        /// </summary>
        [FieldDb(nameof(CustomerId))]
        public long CustomerId { get; set; }

        /// <summary>
        /// Remote Login
        /// </summary>
        [FieldDb(nameof(RemoteLogin))]
        public string RemoteLogin { get; set; }

        /// <summary>
        /// More Hardware
        /// </summary>
        [FieldDb(nameof(MoreHardware))]
        public string MoreHardware { get; set; }

        /// <summary>
        /// Other Notes
        /// </summary>
        [FieldDb(nameof(OtherNotes))]
        public string OtherNotes { get; set; }

        /// <summary>
        /// CreateBy
        /// </summary>
        [FieldDb(nameof(CreateBy))]
        public string CreateBy { get; set; }

        /// <summary>
        /// CreateAt
        /// </summary>
        [FieldDb(nameof(CreateAt))]
        public System.DateTime? CreateAt { get; set; }

        /// <summary>
        /// UpdateBy
        /// </summary>
        [FieldDb(nameof(UpdateBy))]
        public string UpdateBy { get; set; }

        /// <summary>
        /// UpdateAt
        /// </summary>
        [FieldDb(nameof(UpdateAt))]
        public System.DateTime? UpdateAt { get; set; }
    }
}
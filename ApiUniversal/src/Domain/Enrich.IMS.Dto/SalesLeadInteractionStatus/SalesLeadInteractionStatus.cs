using Enrich.Dto.Base.Attributes;
using System;

namespace Enrich.IMS.Dto
{
    [FieldDb(table: $"{SqlTables.SalesLeadInteractionStatus}")]
    public partial class SalesLeadInteractionStatusDto
    {
        /// <summary>
        /// Id
        /// </summary>
        [FieldDb(nameof(Id))]
        public int Id { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        [FieldDb(nameof(Name))]
        public string Name { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        [FieldDb(nameof(Status))]
        public bool? Status { get; set; }

        /// <summary>
        /// Created At
        /// </summary>
        [FieldDb(nameof(CreatedAt))]
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// Created By
        /// </summary>
        [FieldDb(nameof(CreatedBy))]
        public string CreatedBy { get; set; }

        /// <summary>
        /// Updated At
        /// </summary>
        [FieldDb(nameof(UpdatedAt))]
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Updated By
        /// </summary>
        [FieldDb(nameof(UpdatedBy))]
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Order
        /// </summary>
        [FieldDb(nameof(Order))]
        public int? Order { get; set; }
    }
}

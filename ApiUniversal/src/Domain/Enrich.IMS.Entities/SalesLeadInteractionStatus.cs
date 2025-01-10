using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enrich.IMS.Entities
{ 
    public partial class SalesLeadInteractionStatus
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]

        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        public bool? Status { get; set; }

        /// <summary>
        /// Created At
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// Created By
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// Updated At
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Updated By
        /// </summary>
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Order
        /// </summary>
        public int? Order { get; set; }
    }
}

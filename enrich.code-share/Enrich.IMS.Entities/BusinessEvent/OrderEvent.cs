using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enrich.IMS.Entities
{
    public partial class OrderEvent
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Complete At
        /// </summary>
        public DateTime CompleteAt { get; set; }

        /// <summary>
        /// Complete By
        /// </summary>
        public string CompleteBy { get; set; }

        /// <summary>
        /// Business Event Id
        /// </summary>
        public long BusinessEventId { get; set; }
    }
}
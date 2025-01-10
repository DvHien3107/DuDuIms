using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enrich.IMS.Entities
{
    public partial class BusinessEvent
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        /// <summary>
        /// NameSpace
        /// </summary>
        public string NameSpace { get; set; }

        /// <summary>
        /// Event
        /// </summary>
        public string Event { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Meta Data
        /// </summary>
        public string MetaData { get; set; }

        /// <summary>
        /// Create at
        /// </summary>
        public DateTime? CreateAt { get; set; }

        /// <summary>
        /// Create by
        /// </summary>
        public string CreateBy { get; set; }
    }
}
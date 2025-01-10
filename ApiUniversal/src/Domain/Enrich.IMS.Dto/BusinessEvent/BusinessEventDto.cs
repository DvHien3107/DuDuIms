using Enrich.Dto.Base.Attributes;
using System;

namespace Enrich.IMS.Dto
{
    [FieldDb(table: $"{SqlTables.BusinessEvent}")]
    public partial class BusinessEventDto
    {
        [FieldDb(nameof(Id))]
        public string Id { get; set; }

        /// <summary>
        /// NameSpace
        /// </summary>
        [FieldDb(nameof(NameSpace))]
        public string NameSpace { get; set; }

        /// <summary>
        /// Event
        /// </summary>
        [FieldDb(nameof(Event))]
        public string Event { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        [FieldDb(nameof(Description))]
        public string Description { get; set; }

        /// <summary>
        /// Meta Data
        /// </summary>
        [FieldDb(nameof(MetaData))]
        public string MetaData { get; set; }

        /// <summary>
        /// Create at
        /// </summary>
        [FieldDb(nameof(CreateAt))]
        public DateTime? CreateAt { get; set; }

        /// <summary>
        /// Create by
        /// </summary>
        [FieldDb(nameof(CreateBy))]
        public string CreateBy { get; set; }
    }
}
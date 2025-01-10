using Enrich.Dto.Base.Attributes;
using System;

namespace Enrich.IMS.Dto
{
    public partial class BaseEvent
    {
        [FieldDb(nameof(Id))]
        public long Id { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        [FieldDb(nameof(Status))]
        public int Status { get; set; }

        /// <summary>
        /// Complete At
        /// </summary>
        [FieldDb(nameof(CompleteAt))]
        public DateTime? CompleteAt { get; set; }

        /// <summary>
        /// Complete By
        /// </summary>
        [FieldDb(nameof(CompleteBy))]
        public string CompleteBy { get; set; }

        /// <summary>
        /// Business Event Id
        /// </summary>
        [FieldDb(nameof(BusinessEventId))]
        public long BusinessEventId { get; set; }
    }
}
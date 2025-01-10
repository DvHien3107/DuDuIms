using Enrich.Common.Enums;
using Enrich.Dto.Base.Attributes;

namespace Enrich.IMS.Dto
{
    [FieldDb(table: $"{SqlTables.SalesLeadComment}")]
    public class SalesLeadCommentDto
    {
        [FieldDb(nameof(Id))]
        public int Id { get; set; }

        /// <summary>
        /// Title
        /// </summary>
        [FieldDb(nameof(Title))]
        public string Title { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        [FieldDb(nameof(Description))]
        public string Description { get; set; }

        /// <summary>
        /// Sales Lead Id
        /// </summary>
        [FieldDb(nameof(SalesLeadId))]
        public string SalesLeadId { get; set; }

        /// <summary>
        /// Update by
        /// </summary>
        [FieldDb(nameof(UpdateBy))]
        public string UpdateBy { get; set; }

        /// <summary>
        /// Update at
        /// </summary>
        [FieldDb(nameof(UpdateAt))]
        public System.DateTime? UpdateAt { get; set; }

        /// <summary>
        /// Create by
        /// </summary>
        [FieldDb(nameof(CreateBy))]
        public string CreateBy { get; set; }

        /// <summary>
        /// Create at
        /// </summary>
        [FieldDb(nameof(CreateAt))]
        public System.DateTime? CreateAt { get; set; }

        /// <summary>
        /// Create by memberNumber
        /// </summary>
        [FieldDb(nameof(CreateByName))]
        public string CreateByName { get; set; }
    }
}

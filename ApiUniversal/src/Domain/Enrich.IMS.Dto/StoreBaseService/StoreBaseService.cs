using Enrich.Dto.Base.Attributes;
using System;

namespace Enrich.IMS.Dto
{
    [FieldDb(table: $"{SqlTables.StoreBaseService}")]
    public partial class StoreBaseServiceDto
    {
        [FieldDb(nameof(Id))]
        public int Id { get; set; }

        /// <summary>
        /// Store Code
        /// </summary>
        [FieldDb(nameof(StoreCode))]
        public string StoreCode { get; set; }

        /// <summary>
        /// Key Name base service
        /// </summary>
        [FieldDb(nameof(KeyName))]
        public string KeyName { get; set; }

        /// <summary>
        /// Remaining Value
        /// </summary>
        [FieldDb(nameof(RemainingValue))]
        public int RemainingValue { get; set; }

        /// <summary>
        /// Maximum Value
        /// </summary>
        [FieldDb(nameof(MaximumValue))]
        public int MaximumValue { get; set; }

        /// <summary>
        /// CreateAt
        /// </summary>
        [FieldDb(nameof(CreateAt))]
        public DateTime CreateAt { get; set; }

        /// <summary>
        /// CreateBy
        /// </summary>
        [FieldDb(nameof(CreateBy))]
        public string CreateBy { get; set; }

        /// <summary>
        /// UpdateBy
        /// </summary>
        [FieldDb(nameof(UpdateBy))]
        public string UpdateBy { get; set; }

        /// <summary>
        /// UpdateAt
        /// </summary>
        [FieldDb(nameof(UpdateAt))]
        public DateTime UpdateAt { get; set; }
    }
}

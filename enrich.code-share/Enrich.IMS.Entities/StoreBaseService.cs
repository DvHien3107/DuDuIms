using System;

namespace Enrich.IMS.Entities
{
    public partial class StoreBaseService
    {
        public int Id { get; set; }

        /// <summary>
        /// Store Code
        /// </summary>
        public string StoreCode { get; set; }

        /// <summary>
        /// Key Name base service
        /// </summary>
        public string KeyName { get; set; }

        /// <summary>
        /// Remaining Value
        /// </summary>
        public int RemainingValue { get; set; }

        /// <summary>
        /// Maximum Value
        /// </summary>
        public int MaximumValue { get; set; }

        /// <summary>
        /// CreateAt
        /// </summary>
        public DateTime? CreateAt { get; set; }

        /// <summary>
        /// CreateBy
        /// </summary>
        public string CreateBy { get; set; }

        /// <summary>
        /// UpdateBy
        /// </summary>
        public string UpdateBy { get; set; }

        /// <summary>
        /// UpdateAt
        /// </summary>
        public DateTime? UpdateAt { get; set; }
    }
}

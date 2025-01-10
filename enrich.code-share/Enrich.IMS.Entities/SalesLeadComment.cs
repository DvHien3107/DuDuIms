using System.ComponentModel.DataAnnotations.Schema;

namespace Enrich.IMS.Entities
{
    public class SalesLeadComment
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Sales Lead Id
        /// </summary>
        public string SalesLeadId { get; set; }

        /// <summary>
        /// Update by
        /// </summary>
        public string UpdateBy { get; set; }

        /// <summary>
        /// Update at
        /// </summary>
        public System.DateTime? UpdateAt { get; set; }

        /// <summary>
        /// Create by
        /// </summary>
        public string CreateBy { get; set; }

        /// <summary>
        /// Create at
        /// </summary>
        public System.DateTime? CreateAt { get; set; }

    }
}

using System;

namespace Enrich.IMS.Entities
{
    public partial class Department
    {
        /// <summary>
        /// Id 
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Name 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Leader Number 
        /// </summary>
        public string LeaderNumber { get; set; }

        /// <summary>
        /// Leader Name 
        /// </summary>
        public string LeaderName { get; set; }

        /// <summary>
        /// Active 
        /// </summary>
        public bool? Active { get; set; }

        /// <summary>
        /// CreateBy 
        /// </summary>
        public string CreateBy { get; set; }

        /// <summary>
        /// CreateAt 
        /// </summary>
        public DateTime? CreateAt { get; set; }

        /// <summary>
        /// UpdateBy 
        /// </summary>
        public string UpdateBy { get; set; }

        /// <summary>
        /// UpdateAt 
        /// </summary>
        public DateTime? UpdateAt { get; set; }

        /// <summary>
        /// Parent Department Id 
        /// </summary>
        public long? ParentDepartmentId { get; set; }

        /// <summary>
        /// Parent Department Name 
        /// </summary>
        public string ParentDepartmentName { get; set; }

        /// <summary>
        /// Group Member Number 
        /// </summary>
        public string GroupMemberNumber { get; set; }

        /// <summary>
        /// Group Member Name 
        /// </summary>
        public string GroupMemberName { get; set; }

        /// <summary>
        /// Type 
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Supervisor Number 
        /// </summary>
        public string SupervisorNumber { get; set; }

        /// <summary>
        /// Supervisor Name 
        /// </summary>
        public string SupervisorName { get; set; }

        /// <summary>
        /// Sale States 
        /// </summary>
        public string SaleStates { get; set; }

        /// <summary>
        /// Partner Code 
        /// </summary>
        public string PartnerCode { get; set; }

        /// <summary>
        /// Partner Name 
        /// </summary>
        public string PartnerName { get; set; }
    }
}

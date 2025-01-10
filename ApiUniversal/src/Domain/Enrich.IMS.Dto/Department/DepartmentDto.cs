using Enrich.Dto.Base.Attributes;
using System;

namespace Enrich.IMS.Dto
{
    public partial class DepartmentDto
    {

        /// <summary>
        /// Id 
        /// </summary>
        [FieldDb(nameof(Id))]
        public long Id { get; set; }

        /// <summary>
        /// Name 
        /// </summary>
        [FieldDb(nameof(Name))]
        public string Name { get; set; }

        /// <summary>
        /// Leader Number 
        /// </summary>
        [FieldDb(nameof(LeaderNumber))]
        public string LeaderNumber { get; set; }

        /// <summary>
        /// Leader Name 
        /// </summary>
        [FieldDb(nameof(LeaderName))]
        public string LeaderName { get; set; }

        /// <summary>
        /// Active 
        /// </summary>
        [FieldDb(nameof(Active))]
        public bool? Active { get; set; }

        /// <summary>
        /// CreateBy 
        /// </summary>
        [FieldDb(nameof(CreateBy))]
        public string CreateBy { get; set; }

        /// <summary>
        /// CreateAt 
        /// </summary>
        [FieldDb(nameof(CreateAt))]
        public DateTime? CreateAt { get; set; }

        /// <summary>
        /// UpdateBy 
        /// </summary>
        [FieldDb(nameof(UpdateBy))]
        public string UpdateBy { get; set; }

        /// <summary>
        /// UpdateAt 
        /// </summary>
        [FieldDb(nameof(UpdateAt))]
        public DateTime? UpdateAt { get; set; }

        /// <summary>
        /// Parent Department Id 
        /// </summary>
        [FieldDb(nameof(ParentDepartmentId))]
        public long? ParentDepartmentId { get; set; }

        /// <summary>
        /// Parent Department Name 
        /// </summary>
        [FieldDb(nameof(ParentDepartmentName))]
        public string ParentDepartmentName { get; set; }

        /// <summary>
        /// Group Member Number 
        /// </summary>
        [FieldDb(nameof(GroupMemberNumber))]
        public string GroupMemberNumber { get; set; }

        /// <summary>
        /// Group Member Name 
        /// </summary>
        [FieldDb(nameof(GroupMemberName))]
        public string GroupMemberName { get; set; }

        /// <summary>
        /// Type 
        /// </summary>
        [FieldDb(nameof(Type))]
        public string Type { get; set; }

        /// <summary>
        /// Supervisor Number 
        /// </summary>
        [FieldDb(nameof(SupervisorNumber))]
        public string SupervisorNumber { get; set; }

        /// <summary>
        /// Supervisor Name 
        /// </summary>
        [FieldDb(nameof(SupervisorName))]
        public string SupervisorName { get; set; }

        /// <summary>
        /// Sale States 
        /// </summary>
        [FieldDb(nameof(SaleStates))]
        public string SaleStates { get; set; }

        /// <summary>
        /// Partner Code 
        /// </summary>
        [FieldDb(nameof(PartnerCode))]
        public string PartnerCode { get; set; }

        /// <summary>
        /// Partner Name 
        /// </summary>
        [FieldDb(nameof(PartnerName))]
        public string PartnerName { get; set; }
    }
}
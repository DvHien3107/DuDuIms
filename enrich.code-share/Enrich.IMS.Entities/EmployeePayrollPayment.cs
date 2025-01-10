


//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>


namespace Enrich.IMS.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class EmployeePayrollPayment
    {
        public int Id { get; set; }
        public string PaidNumber { get; set; }
        public string PaymentMethod { get; set; }
        public string CardNumber { get; set; }
        public string Description { get; set; }
        public string GroupMemberNumber { get; set; }
        public string GroupMemberName { get; set; }
        public System.DateTime? CreateAt { get; set; }
        public string CreateBy { get; set; }
        public string Month { get; set; }
        public string PaymentName { get; set; }
        public System.DateTime? PaymentDate { get; set; }
        public System.DateTime? UpdateAt { get; set; }
        public string UpdateBy { get; set; }
        public string Comment { get; set; }
    }
}

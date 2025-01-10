using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel
{
    public partial class DetailEmployeePayrollCustomizeModel
    {
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public bool? IsSalesPerson { get; set; }
        public decimal? BaseSalary { get; set; }
        public bool? IsCalculatedBaseSalary { get; set; }
        public long? EmpPayrollBaseSalaryId { get; set; }
        public bool? PaidBaseSalary { get; set; }
        public bool? SetAsPaid { get; set; }
        public string Department { get; set; }
        public List<EmployeeComission> ListEmployeeComission { get; set; }
        public PercentCommission PercentCommission { get; set; }
    }
    public partial class EmployeeComission
    {
        public long? EmpPayrollId { get; set; }
        public string TypeOfInvoice { get; set; }
        public string Type { get; set; }
        public string OrderCode { get; set; }
        public string CustomerName { get; set; }
        public string SalesPersonName { get; set; }
        public string SalesPersonNumber { get; set; }
        public string TransactionId { get; set; }
        public decimal? Amount { get; set; }
        public string Status { get; set; }
        public bool? IsCalculated { get; set; }
        public bool? Paid { get; set; }
        public decimal? Comisssion { get; set; }
        public decimal? PaidCommission { get; set; }
        public decimal? BalanceCommissionAmount { get; set; }
    }

    public partial class PercentCommission
    {
        public decimal? PercentComissionMember { get; set; }
        public decimal? PercentComissionLeader { get; set; }
        public decimal? PercentComissionManager { get; set; }
        public decimal? PercentComissionDirector { get; set; }
    }
    //detail 
    public partial class DetailPayrollPayment
    {
        public string PaidNumber { get; set; }
        public string PaidDescription { get; set; }
        public string PaidInfo { get; set; }

        public List<EmployeeDetailPayment> ListEmployee { get; set; }
    }
    public partial class EmployeeDetailPayment
    {
        public string MemberNumber { get; set;}
        public string MemberName { get; set; }
        public decimal? TotalSalary { get; set; }
        public decimal? TotalCommission { get; set; }
        public string PaidDate { get; set; }

        public string PaidMethod { get; set; }
    }
}
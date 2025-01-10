
namespace Enrich.IMS.Entities
{
    public partial class EmployeePayroll
    {
        public long Id { get; set; }
        public string RecipientMemberNumber { get; set; }
        public string RecipientName { get; set; }
        public decimal? Amount { get; set; }
        public string Comment { get; set; }
        public string ContractId { get; set; }
        public string CustomerTransactionId { get; set; }
        public string CustomerName { get; set; }
        public string Type { get; set; }
        public string TypeComm { get; set; }
        public bool? OverrideComm { get; set; }
        public bool? Paid { get; set; }
        public string PaidNumber { get; set; }
        public string PaidInfo { get; set; }
        public string TakeBackMoney_Comment { get; set; }
        public string TakeBackMoney_ExceptFromPaidNumber { get; set; }
        public bool? TakeBackMoney { get; set; }
        public string PayrollMonth { get; set; }
        public System.DateTime? CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public bool? IsCalculate { get; set; }
        public decimal? TransactionAmount { get; set; }
    }
}

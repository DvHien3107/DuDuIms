namespace Enrich.IMS.Entities
{    
    public partial class PayrollFiles
    {
        public int Id { get; set; }
        public System.DateTime PayrollDate { get; set; }
        public string FilePath { get; set; }
        public string MemberNumber { get; set; }
        public bool? IsSendMail { get; set; }
        public System.DateTime? CreateAt { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime? UpdateAt { get; set; }
        public string UpdateBy { get; set; }
        public bool? IsApproved { get; set; }
        public string FileName { get; set; }
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public bool? IsActive { get; set; }
    }
}

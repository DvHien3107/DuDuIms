
namespace Enrich.IMS.Entities
{   
    public partial class BankSupport
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Account { get; set; }
        public short? Type { get; set; }
        public string Method { get; set; }
        public string Logo { get; set; }
        public string Url { get; set; }
        public short? Status { get; set; }
        public bool? IsNapas { get; set; }
        public System.DateTime? CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime? UpdatedAt { get; set; }
        public string UpdateBy { get; set; }
        public string BankDDA { get; set; }
        public string BankAccountRouting { get; set; }
        public string Comment { get; set; }
    }
}

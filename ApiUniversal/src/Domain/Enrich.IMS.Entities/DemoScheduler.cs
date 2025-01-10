
namespace Enrich.IMS.Entities
{
    public partial class DemoScheduler
    {
        public int Id { get; set; }
        public string CustomerCode { get; set; }
        public string Note { get; set; }
        public System.DateTime? CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime? UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public string AttendeesNumber { get; set; }
        public string AttendeesName { get; set; }
        public int? Status { get; set; }
    }
}

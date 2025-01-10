using System;

namespace Enrich.IMS.Entities
{
    public partial class RecurringHistory
    {
        public int Id { get; set; }
        public int? RecurringId { get; set; }
        public string OldOrderCode { get; set; }
        public string RecurringOrder { get; set; }
        public int? Status { get; set; }
        public string Message { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public decimal? TotalPrice { get; set; }
    }
}

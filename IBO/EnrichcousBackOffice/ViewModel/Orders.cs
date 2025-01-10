using EnrichcousBackOffice.Models;

namespace EnrichcousBackOffice.ViewModel
{
    public class VmOrderService : Order_Subcription
    {
        public decimal Promotion_Price { get; set; }
        public int Promotion_Apply_Months { get; set; }
        public int TrialMonths { get; set; }
        public int Count { get; set; }
        public decimal Amount { get; set; }
    }
}
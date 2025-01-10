using System;

namespace EnrichcousBackOffice.Areas.PaymentGate.ModelsView.DTO
{
    public class Transaction_view
    {
        public string Id { get; set; }
        public string status { get; set; }
        public DateTime? createAt { get; set; }
        public string createBy { get; set; }
        public string order { get; set; }
        public string type { get; set; }
        public string paymentNote { get; set; }
        public string updateNote { get; set; }
        public decimal amount { get; set; }
        public string Card_id { get; set; }
        public string bankName { get; set; }
        public string cardNumber { get; set; }
        public string responeText { get; set; }
        public DateTime? invoice_date { get; internal set; }
        public string noty { get; internal set; }
        public bool order_pending { get; internal set; }
        public string CreateAtStr { get; set; }
    }

}
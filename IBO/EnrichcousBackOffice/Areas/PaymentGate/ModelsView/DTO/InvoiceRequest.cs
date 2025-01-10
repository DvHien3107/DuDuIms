using System.Collections.Generic;

namespace EnrichcousBackOffice.Areas.PaymentGate.ModelsView.DTO
{
    public class InvoiceRequest
    {
        public List<InvoiceRequestDetail> Invoices { get; set; }
        public decimal GrandTotal  { get; set; }
        public string LastOrder { get; set; } = "";
        public string AgentKey { get; set; } = "";
    }
    
    public class InvoiceRequestDetail
    {
        public string Type { get; set; }
        public string Code { get; set; }
        public decimal Price { get; set; }
        public decimal SubTotal { get; set; }
        public int Quantity { get; set; }
    }

}
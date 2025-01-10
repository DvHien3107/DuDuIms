using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Model.Model.Table.IMS
{
    public class O_Orders
    {
        public long Id { get; set; }

        public string OrdersCode { get; set; }

        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public string SalesMemberNumber { get; set; }

        public string SalesName { get; set; }

        public decimal? GrandTotal { get; set; }

        public string Comment { get; set; }

        public bool? Approved { get; set; }

        public string ApprovedBy { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public DateTime? CreatedAt { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public string UpdatedHistory { get; set; }

        public string Status { get; set; }

        public decimal? TotalHardware_Amount { get; set; }

        public decimal? ShippingFee { get; set; }

        public decimal? Service_Amount { get; set; }

        public decimal? DiscountAmount { get; set; }

        public double? DiscountPercent { get; set; }

        public bool? IsDelete { get; set; }

        public DateTime? ShippingDate { get; set; }

        public decimal? OtherFee { get; set; }

        public decimal? TaxRate { get; set; }

        public string CreateByMemNumber { get; set; }

        public string ShipVIA { get; set; }

        public string ShippingAddress { get; set; }

        public string BundelStatus { get; set; }

        public string StatusHistory { get; set; }

        public bool? Cancel { get; set; }

        public string Note_Packaging { get; set; }

        public string Note_Delivery { get; set; }

        public long? InvoiceNumber { get; set; }

        public string DeploymentStatus { get; set; }

        public DateTime? StartDate { get; set; }

        public string UpdatedBy { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime? InvoiceDate { get; set; }

        public bool? Renewal { get; set; }

        public string PaymentMethod { get; set; }

        public string PaymentNote { get; set; }

        public string PartnerCode { get; set; }

        public bool? CreateDeployTicket { get; set; }

    }
}

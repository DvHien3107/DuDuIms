using Enrich.Dto.Base.Attributes;
using System;

namespace Enrich.IMS.Dto
{
    public partial class OrderDto
    {
        /// <summary>
        /// Id
        /// </summary>
        [FieldDb(nameof(Id))]
        public long Id { get; set; }

        /// <summary>
        /// Order Code
        /// </summary>
        [FieldDb(nameof(OrdersCode))]
        public string OrdersCode { get; set; }

        /// <summary>
        /// Customer Code
        /// </summary>
        [FieldDb(nameof(CustomerCode))]
        public string CustomerCode { get; set; }

        /// <summary>
        /// Customer Name
        /// </summary>
        [FieldDb(nameof(CustomerName))]
        public string CustomerName { get; set; }

        /// <summary>
        /// Sales Member Number
        /// </summary>
        [FieldDb(nameof(SalesMemberNumber))]
        public string SalesMemberNumber { get; set; }

        /// <summary>
        /// Sales Name
        /// </summary>
        [FieldDb(nameof(SalesName))]
        public string SalesName { get; set; }

        /// <summary>
        /// Grand Total
        /// </summary>
        [FieldDb(nameof(GrandTotal))]
        public decimal? GrandTotal { get; set; }

        /// <summary>
        /// Comment
        /// </summary>
        [FieldDb(nameof(Comment))]
        public string Comment { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        [FieldDb(nameof(Approved))]
        public bool? Approved { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        [FieldDb(nameof(ApprovedBy))]
        public string ApprovedBy { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        [FieldDb(nameof(ApprovedAt))]
        public System.DateTime? ApprovedAt { get; set; }

        /// <summary>
        /// Created At
        /// </summary>
        [FieldDb(nameof(CreatedAt))]
        public System.DateTime? CreatedAt { get; set; }

        /// <summary>
        /// Created By
        /// </summary>
        [FieldDb(nameof(CreatedBy))]
        public string CreatedBy { get; set; }

        /// <summary>
        /// Updated At
        /// </summary>
        [FieldDb(nameof(UpdatedAt))]
        public System.DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Updated History
        /// </summary>
        [FieldDb(nameof(UpdatedHistory))]
        public string UpdatedHistory { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        [FieldDb(nameof(Status))]
        public string Status { get; set; }

        /// <summary>
        /// Total Hardware Amount
        /// </summary>
        [FieldDb($"{SqlColumns.Orders.TotalHardwareAmount}")]
        public decimal? TotalHardwareAmount { get; set; }

        /// <summary>
        /// Shipping Fee
        /// </summary>
        [FieldDb(nameof(ShippingFee))]
        public decimal? ShippingFee { get; set; }

        /// <summary>
        /// Service Amount
        /// </summary>
        [FieldDb($"{SqlColumns.Orders.ServiceAmount}")]
        public decimal? ServiceAmount { get; set; }

        /// <summary>
        /// Discount Amount
        /// </summary>
        [FieldDb(nameof(DiscountAmount))]
        public decimal? DiscountAmount { get; set; }

        /// <summary>
        /// Discount Percent
        /// </summary>
        [FieldDb(nameof(DiscountPercent))]
        public double? DiscountPercent { get; set; }

        /// <summary>
        /// Is Delete
        /// </summary>
        [FieldDb(nameof(IsDelete))]
        public bool? IsDelete { get; set; }

        /// <summary>
        /// Shipping Date
        /// </summary>
        [FieldDb(nameof(ShippingDate))]
        public System.DateTime? ShippingDate { get; set; }

        /// <summary>
        /// Other Fee
        /// </summary>
        [FieldDb(nameof(OtherFee))]
        public decimal? OtherFee { get; set; }

        /// <summary>
        /// Tax Rate
        /// </summary>
        [FieldDb(nameof(TaxRate))]
        public decimal? TaxRate { get; set; }

        /// <summary>
        /// Create By Member Number
        /// </summary>
        [FieldDb(nameof(CreateByMemNumber))]
        public string CreateByMemNumber { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        [FieldDb(nameof(ShipVIA))]
        public string ShipVIA { get; set; }

        /// <summary>
        /// Shipping Address
        /// </summary>
        [FieldDb(nameof(ShippingAddress))]
        public string ShippingAddress { get; set; }

        /// <summary>
        /// Bundel Status
        /// </summary>
        [FieldDb(nameof(BundelStatus))]
        public string BundelStatus { get; set; }

        /// <summary>
        /// Status History
        /// </summary>
        [FieldDb(nameof(StatusHistory))]
        public string StatusHistory { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        [FieldDb(nameof(Cancel))]
        public bool? Cancel { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        [FieldDb($"{SqlColumns.Orders.NotePackaging}")]
        public string NotePackaging { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        [FieldDb($"{SqlColumns.Orders.NoteDelivery}")]
        public string NoteDelivery { get; set; }

        /// <summary>
        /// Invoice Number
        /// </summary>
        [FieldDb(nameof(InvoiceNumber))]
        public long? InvoiceNumber { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        [FieldDb(nameof(DeploymentStatus))]
        public string DeploymentStatus { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        [FieldDb(nameof(StartDate))]
        public System.DateTime? StartDate { get; set; }

        /// <summary>
        /// Updated By
        /// </summary>
        [FieldDb(nameof(UpdatedBy))]
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Due Date for payment later
        /// </summary>
        [FieldDb(nameof(DueDate))]
        public System.DateTime? DueDate { get; set; }

        /// <summary>
        /// Invoice Date
        /// </summary>
        [FieldDb(nameof(InvoiceDate))]
        public System.DateTime? InvoiceDate { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        [FieldDb(nameof(Renewal))]
        public bool? Renewal { get; set; }

        /// <summary>
        /// Payment Method
        /// </summary>
        [FieldDb(nameof(PaymentMethod))]
        public string PaymentMethod { get; set; }

        /// <summary>
        /// Payment Note
        /// </summary>
        [FieldDb(nameof(PaymentNote))]
        public string PaymentNote { get; set; }

        /// <summary>
        /// Partner Code
        /// </summary>
        [FieldDb(nameof(PartnerCode))]
        public string PartnerCode { get; set; }

        /// <summary>
        /// Create Deploy Ticket
        /// </summary>
        [FieldDb(nameof(CreateDeployTicket))]
        public bool? CreateDeployTicket { get; set; }
    }
}

using System.ComponentModel.DataAnnotations.Schema;

namespace Enrich.IMS.Entities
{
    public partial class Order
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]

        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Order Code
        /// </summary>
        public string OrdersCode { get; set; }

        /// <summary>
        /// Customer Code
        /// </summary>
        public string CustomerCode { get; set; }

        /// <summary>
        /// Customer Name
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// Sales Member Number
        /// </summary>
        public string SalesMemberNumber { get; set; }

        /// <summary>
        /// Sales Name
        /// </summary>
        public string SalesName { get; set; }

        /// <summary>
        /// Grand Total
        /// </summary>
        public decimal? GrandTotal { get; set; }

        /// <summary>
        /// Comment
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        public bool? Approved { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        public string ApprovedBy { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        public System.DateTime? ApprovedAt { get; set; }

        /// <summary>
        /// Created At
        /// </summary>
        public System.DateTime? CreatedAt { get; set; }

        /// <summary>
        /// Created By
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// Updated At
        /// </summary>
        public System.DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Updated History
        /// </summary>
        public string UpdatedHistory { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Total Hardware Amount
        /// </summary>
        public decimal? TotalHardwareAmount { get; set; }

        /// <summary>
        /// Shipping Fee
        /// </summary>
        public decimal? ShippingFee { get; set; }

        /// <summary>
        /// Service Amount
        /// </summary>
        public decimal? ServiceAmount { get; set; }

        /// <summary>
        /// Discount Amount
        /// </summary>
        public decimal? DiscountAmount { get; set; }

        /// <summary>
        /// Discount Percent
        /// </summary>
        public double? DiscountPercent { get; set; }

        /// <summary>
        /// Is Delete
        /// </summary>
        public bool? IsDelete { get; set; }

        /// <summary>
        /// Shipping Date
        /// </summary>
        public System.DateTime? ShippingDate { get; set; }

        /// <summary>
        /// Other Fee
        /// </summary>
        public decimal?OtherFee { get; set; }

        /// <summary>
        /// Tax Rate
        /// </summary>
        public decimal? TaxRate { get; set; }

        /// <summary>
        /// Create By Member Number
        /// </summary>
        public string CreateByMemNumber { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        public string ShipVIA { get; set; }

        /// <summary>
        /// Shipping Address
        /// </summary>
        public string ShippingAddress { get; set; }

        /// <summary>
        /// Bundel Status
        /// </summary>
        public string BundelStatus { get; set; }

        /// <summary>
        /// Status History
        /// </summary>
        public string StatusHistory { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        public bool? Cancel { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        public string NotePackaging { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        public string NoteDelivery { get; set; }

        /// <summary>
        /// Invoice Number
        /// </summary>
        public long? InvoiceNumber { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        public string DeploymentStatus { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        public System.DateTime? StartDate { get; set; }

        /// <summary>
        /// Updated By
        /// </summary>
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Due Date for payment later
        /// </summary>
        public System.DateTime? DueDate { get; set; }

        /// <summary>
        /// Invoice Date
        /// </summary>
        public System.DateTime? InvoiceDate { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        public bool? Renewal { get; set; }

        /// <summary>
        /// Payment Method
        /// </summary>
        public string PaymentMethod { get; set; }

        /// <summary>
        /// Payment Note
        /// </summary>
        public string PaymentNote { get; set; }

        /// <summary>
        /// Partner Code
        /// </summary>
        public string PartnerCode { get; set; }

        /// <summary>
        /// Create Deploy Ticket
        /// </summary>
        public bool? CreateDeployTicket { get; set; }
    }
}

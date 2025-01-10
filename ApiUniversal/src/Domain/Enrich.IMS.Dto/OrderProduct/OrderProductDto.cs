using Enrich.Dto.Base.Attributes;

namespace Enrich.IMS.Dto
{   
    public partial class OrderProductDto
    {
        public long Id { get; set; }
        /// <summary>
        /// Order Code
        /// </summary>
        [FieldDb(nameof(OrderCode))]
        public string OrderCode { get; set; }

        /// <summary>
        /// Product Code
        /// </summary>
        [FieldDb(nameof(ProductCode))]
        public string ProductCode { get; set; }

        /// <summary>
        /// Product Name
        /// </summary>
        [FieldDb(nameof(ProductName))]
        public string ProductName { get; set; }

        /// <summary>
        /// Quantity
        /// </summary>
        [FieldDb(nameof(Quantity))]
        public int? Quantity { get; set; }

        /// <summary>
        /// Price
        /// </summary>
        [FieldDb(nameof(Price))]
        public decimal? Price { get; set; }

        /// <summary>
        /// Total Amount
        /// </summary>
        [FieldDb(nameof(TotalAmount))]
        public decimal? TotalAmount { get; set; }

        /// <summary>
        /// CreateBy
        /// </summary>
        [FieldDb(nameof(CreateBy))]
        public string CreateBy { get; set; }

        /// <summary>
        /// CreateAt
        /// </summary>
        [FieldDb(nameof(CreateAt))]
        public System.DateTime? CreateAt { get; set; }

        /// <summary>
        /// UpdateBy
        /// </summary>
        [FieldDb(nameof(UpdateBy))]
        public string UpdateBy { get; set; }

        /// <summary>
        /// UpdateAt
        /// </summary>
        [FieldDb(nameof(UpdateAt))]
        public System.DateTime? UpdateAt { get; set; }

        /// <summary>
        /// Feature
        /// </summary>
        [FieldDb(nameof(Feature))]
        public string Feature { get; set; }

        /// <summary>
        /// Bundle Id
        /// </summary>
        [FieldDb(nameof(BundleId))]
        public long? BundleId { get; set; }

        /// <summary>
        /// Model Code
        /// </summary>
        [FieldDb(nameof(ModelCode))]
        public string ModelCode { get; set; }

        /// <summary>
        /// Inventory Numbers
        /// </summary>
        [FieldDb(nameof(InvNumbers))]
        public string InvNumbers { get; set; }

        /// <summary>
        /// BundleQTY
        /// </summary>
        [FieldDb(nameof(BundleQTY))]
        public int? BundleQTY { get; set; }

        /// <summary>
        /// ModelName
        /// </summary>
        [FieldDb(nameof(ModelName))]
        public string ModelName { get; set; }

        /// <summary>
        /// Serial Numbers
        /// </summary>
        [FieldDb(nameof(SerNumbers))]
        public string SerNumbers { get; set; }

        /// <summary>
        /// Discount
        /// </summary>
        [FieldDb(nameof(Discount))]
        public decimal? Discount { get; set; }

        /// <summary>
        /// Discount Percent
        /// </summary>
        [FieldDb(nameof(DiscountPercent))]
        public decimal? DiscountPercent { get; set; }

        /// <summary>
        /// Custom Numbers
        /// </summary>
        [FieldDb(nameof(CusNumbers))]
        public string CusNumbers { get; set; }
    }
}

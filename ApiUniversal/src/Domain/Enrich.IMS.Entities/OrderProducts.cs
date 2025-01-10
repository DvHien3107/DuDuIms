using System.ComponentModel.DataAnnotations.Schema;

namespace Enrich.IMS.Entities
{    
    public partial class OrderProducts
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Order Code
        /// </summary>
        public string OrderCode { get; set; }

        /// <summary>
        /// Product Code
        /// </summary>
        public string ProductCode { get; set; }

        /// <summary>
        /// Product Name
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// Quantity
        /// </summary>
        public int? Quantity { get; set; }

        /// <summary>
        /// Price
        /// </summary>
        public decimal? Price { get; set; }

        /// <summary>
        /// Total Amount
        /// </summary>
        public decimal? TotalAmount { get; set; }

        /// <summary>
        /// CreateBy
        /// </summary>
        public string CreateBy { get; set; }

        /// <summary>
        /// CreateAt
        /// </summary>
        public System.DateTime? CreateAt { get; set; }

        /// <summary>
        /// UpdateBy
        /// </summary>
        public string UpdateBy { get; set; }

        /// <summary>
        /// UpdateAt
        /// </summary>
        public System.DateTime? UpdateAt { get; set; }

        /// <summary>
        /// Feature
        /// </summary>
        public string Feature { get; set; }

        /// <summary>
        /// Bundle Id
        /// </summary>
        public long? BundleId { get; set; }

        /// <summary>
        /// Model Code
        /// </summary>
        public string ModelCode { get; set; }

        /// <summary>
        /// Inventory Numbers
        /// </summary>
        public string InvNumbers { get; set; }

        /// <summary>
        /// BundleQTY
        /// </summary>
        public int? BundleQTY { get; set; }

        /// <summary>
        /// ModelName
        /// </summary>
        public string ModelName { get; set; }

        /// <summary>
        /// Serial Numbers
        /// </summary>
        public string SerNumbers { get; set; }

        /// <summary>
        /// Discount
        /// </summary>
        public decimal? Discount { get; set; }

        /// <summary>
        /// Discount Percent
        /// </summary>
        public decimal? DiscountPercent { get; set; }

        /// <summary>
        /// Custom Numbers
        /// </summary>
        public string CusNumbers { get; set; }
    }
}

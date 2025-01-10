using Enrich.Dto.Attributes;
using Enrich.Dto.Base;
using System;

namespace Enrich.IMS.Dto.OrderSubscription
{
    public partial class OrderSubscriptionListItemDto : ListItemDto
    {
        #region Order subscription infomation

        [GridField(Index = 1, ColumnName = "Id", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.OrderSubscription}.Id")]
        public string Id { get; set; }

        [GridField(Index = 1, ColumnName = "ProductId", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.OrderSubscription}.ProductId")]
        public string ProductId { get; set; }

        [GridField(Index = 1, ColumnName = "ProductName", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.OrderSubscription}.ProductName")]
        public string ProductName { get; set; }

        [GridField(Index = 1, ColumnName = "ProductCode", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.OrderSubscription}.{SqlColumns.OrderSubcription.ProductCode}")]
        public string ProductCode { get; set; }

        [GridField(Index = 1, ColumnName = "Price", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.OrderSubscription}.Price")]
        public decimal? Price { get; set; }

        [GridField(Index = 1, ColumnName = "SubscriptionAmount", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto(@$"CASE WHEN {SqlTables.OrderSubscription}.Amount IS NOT NULL THEN {SqlTables.OrderSubscription}.Amount ELSE ({SqlTables.OrderSubscription}.Price * (CASE WHEN {SqlTables.OrderSubscription}.Period = 'MONTHLY' 
				THEN (CASE WHEN {SqlTables.OrderSubscription}.SubscriptionQuantity IS NOT NULL THEN {SqlTables.OrderSubscription}.SubscriptionQuantity ELSE 1 END) 
				ELSE (CASE WHEN {SqlTables.OrderSubscription}.Quantity IS NOT NULL THEN {SqlTables.OrderSubscription}.Quantity ELSE 1 END) END) - (CASE WHEN {SqlTables.OrderSubscription}.Discount IS NOT NULL THEN {SqlTables.OrderSubscription}.Discount ELSE 0 END)) END")]
        public decimal? SubscriptionAmount { get; set; }

        [GridField(Index = 1, ColumnName = "Discount", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.OrderSubscription}.Discount")]
        public decimal? Discount { get; set; }

        [GridField(Index = 1, ColumnName = "DiscountPercent", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.OrderSubscription}.DiscountPercent")]
        public decimal? DiscountPercent { get; set; }

        [GridField(Index = 1, ColumnName = "Quantity", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"CASE WHEN {SqlTables.OrderSubscription}.Period = 'MONTHLY' THEN {SqlTables.OrderSubscription}.SubscriptionQuantity ELSE {SqlTables.OrderSubscription}.Quantity END")]
        public int? Quantity { get; set; }

        [GridField(Index = 1, ColumnName = "PriceType", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.OrderSubscription}.PriceType")]
        public string PriceType { get; set; }

        [GridField(Index = 1, ColumnName = "Type", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.OrderSubscription}.SubscriptionType")]
        public string Type { get; set; }

        #endregion

        #region Customer information

        [GridField(Index = 1, ColumnName = "CustomerId", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.Customer}.Id", sqlJoinKeys: SqlJoinKeys.Customer)]
        public string CustomerId { get; set; }

        [GridField(Index = 1, ColumnName = "StoreCode", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.Customer}.StoreCode", sqlJoinKeys: SqlJoinKeys.Customer)]
        public string StoreCode { get; set; }

        [GridField(Index = 1, ColumnName = "StoreName", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.Customer}.BusinessName", sqlJoinKeys: SqlJoinKeys.Customer)]
        public string StoreName { get; set; }

        [GridField(Index = 1, ColumnName = "PartnerCode", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.Customer}.PartnerCode", sqlJoinKeys: SqlJoinKeys.Customer)]
        public string PartnerCode { get; set; }

        [GridField(Index = 1, ColumnName = "PartnerName", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.Customer}.PartnerName", sqlJoinKeys: SqlJoinKeys.Customer)]
        public string PartnerName { get; set; }

        #endregion

        #region Order information

        [GridField(Index = 1, ColumnName = "OrderCode", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.Orders}.OrdersCode")]
        public string OrderCode { get; set; }

        [GridField(Index = 1, ColumnName = "GrandTotal", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.Orders}.GrandTotal")]
        public string OrderTotal { get; set; }

        [GridField(Index = 1, ColumnName = "TotalHardware_Amount", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.Orders}.TotalHardware_Amount")]
        public string OrderAmount { get; set; }

        [GridField(Index = 1, ColumnName = "DiscountAmount", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.Orders}.DiscountAmount")]
        public string OrderDiscountAmount { get; set; }

        [GridField(Index = 1, ColumnName = "DiscountPercent", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.Orders}.DiscountPercent")]
        public string OrderDiscountPercent { get; set; }

        [GridField(Index = 1, ColumnName = "OrderStatus", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.Orders}.Status")]
        public string OrderStatus { get; set; }
        
        [GridField(Index = 1, ColumnName = "CreateAt", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.Orders}.CreatedAt")]
        public DateTime? CreateAt { get; set; }

        [GridField(Index = 1, ColumnName = "CreateBy", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.Orders}.CreatedBy")]
        public string CreateBy { get; set; }

        #endregion

        #region Transaction

        [GridField(Index = 1, ColumnName = "PaymentMethod", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.CustomerTransaction}.PaymentMethod", sqlJoinKeys: SqlJoinKeys.Transaction)]
        public string PaymentMethod { get; set; }

        [GridField(Index = 1, ColumnName = "PaymentDescription", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.CustomerTransaction}.PaymentNote", sqlJoinKeys: SqlJoinKeys.Transaction)]
        public string PaymentDescription { get; set; }

        [GridField(Index = 1, ColumnName = "PaymentBy", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.CustomerTransaction}.CreateBy", sqlJoinKeys: SqlJoinKeys.Transaction)]
        public string PaymentBy { get; set; }

        [GridField(Index = 1, ColumnName = "PaymentDate", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"CASE WHEN {SqlTables.CustomerTransaction}.[CreateAt] IS NULL THEN {SqlTables.Orders}.CreatedAt ELSE {SqlTables.CustomerTransaction}.[CreateAt] END", sqlJoinKeys: SqlJoinKeys.Transaction)]
        public string PaymentDate { get; set; }

        #endregion

        public int? MaxRows { get; set; }

        public decimal? TotalAmount { get; set; }
        public decimal? Income { get; set; }
    }
}
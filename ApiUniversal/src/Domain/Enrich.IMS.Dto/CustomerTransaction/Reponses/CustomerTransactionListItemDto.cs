using Enrich.Dto.Attributes;
using Enrich.Dto.Base;
using System;

namespace Enrich.IMS.Dto.CustomerTransaction
{
    public partial class CustomerTransactionListItemDto : ListItemDto
    {
        #region Transaction information
        [GridField(Index = 1, ColumnName = "Id", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.CustomerTransaction}.Id")]
        public string Id { get; set; }

        [GridField(Index = 1, ColumnName = "CardId", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.CustomerTransaction}.Card")]
        public string CardId { get; set; }

        [GridField(Index = 1, ColumnName = "CardNumber", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.CustomerTransaction}.CardNumber")]
        public string CardNumber { get; set; }

        [GridField(Index = 1, ColumnName = "PaymentStatus", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.CustomerTransaction}.PaymentStatus")]
        public string PaymentStatus { get; set; }

        [GridField(Index = 1, ColumnName = "ResponseText", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.CustomerTransaction}.ResponseText")]
        public string ResponseText { get; set; }

        [GridField(Index = 1, ColumnName = "Amount", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.CustomerTransaction}.Amount")]
        public decimal? Amount { get; set; }

        [GridField(Index = 1, ColumnName = "CreateAt", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.CustomerTransaction}.CreateAt")]
        public DateTime? CreateAt { get; set; }

        [GridField(Index = 1, ColumnName = "CreateBy", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.CustomerTransaction}.CreateBy")]
        public string CreateBy { get; set; }

        [GridField(Index = 1, ColumnName = "PaymentMethod", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.CustomerTransaction}.PaymentMethod")]
        public string PaymentMethod { get; set; }

        [GridField(Index = 1, ColumnName = "PaymentDescription", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.CustomerTransaction}.PaymentNote")]
        public string PaymentDescription { get; set; }

        #endregion


        #region Customer information

        [GridField(Index = 1, ColumnName = "StoreCode", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.Customer}.StoreCode")]
        public string StoreCode { get; set; }

        [GridField(Index = 1, ColumnName = "StoreName", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.Customer}.BusinessName")]
        public string StoreName { get; set; }

        #endregion

        #region Order information

        [GridField(Index = 1, ColumnName = "OrderCode", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.Orders}.OrdersCode")]
        public string OrderCode { get; set; }

        [GridField(Index = 1, ColumnName = "OrderStatus", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.Orders}.Status")]
        public string OrderStatus { get; set; }

        #endregion

        #region Order subscription infomation

        [GridField(Index = 1, ColumnName = "ProductId", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.OrderSubscription}.ProductId")]
        public string ProductId { get; set; }

        [GridField(Index = 1, ColumnName = "ProductName", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.OrderSubscription}.ProductName")]
        public string ProductName { get; set; }

        [GridField(Index = 1, ColumnName = "ProductCode", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($"{SqlTables.OrderSubscription}.{SqlColumns.OrderSubcription.ProductCode}", sqlJoinKeys: "TransactionReport")]
        public string ProductCode { get; set; }

        #endregion
    }
}

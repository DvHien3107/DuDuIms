using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel
{
    public class I_Bundle_Device_view
    {
        public string ProductName { get; set; }
        public string ModelCode { get; set; }
        public string ModelName { get; set; }
        public int? Quantity { get; set; }
        public string Color { get; internal set; }
        public string Vendor { get; internal set; }
    }

    public class I_Bundle_view
    {
        public long Id { get; set; }
        public string BundleCode { get; set; }
        public string Info { get; set; }
        public List<I_Bundle_Device_view> list_model { get; set; }
        public DateTime? UpdateAt { get; internal set; }
        public string UpdateBy { get; internal set; }
        public string Name { get; internal set; }
        public decimal? Price { get; internal set; }
    }
    public class ProductPicture
    {
        public string Code { get; set; }
        public string Picture { get; set; }
    }
    public class O_Product_Model_Selected_view
    {
        public string ModelCode { get; set; }
        public string ModelName { get; set; }
        public string ProductName { get; set; }
        public string VendorName { get; set; }
        public string Color { get; set; }
        public int? Quantity { get; set; }
        public string ProductCode { get; internal set; }
        public string Picture { get; internal set; }
        public decimal? Price { get; internal set; }
      
    }

    public class OrderPackage_view
    {
        public long Id { get; set; }
        public string OrdersCode { get; set; }
        public string Status { get; set; }
        public string CustomerName { get; set; }
        public string BundelStatus { get; set; }
        public string UpdatedBy { get; set; }
        public List<package> Packages { get; set; }
        public decimal? GrandTotal { get; internal set; }
        public long TicketCode { get; set; }
    }
    public class package
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }
    public class OrderProgress_view
    {
        public long? BundleId { get; set; }
        public string BundleCode { get; set; }
        public string BundleName { get; set; }
        public List<OrderProgressDetail_view> Contains { get; set; }
    }

    public class OrderProgressDetail_view
    {
        public long OrderProductId { get; set; }
        public long OrderId { get; set; }
        public string ProductName { get; set; }
        public string ModelCode { get; set; }
        public string ModelName { get; set; }
        public int? Quantity { get; set; }
        public decimal? Price { get; set; }
        public string VendorName { get; set; }
        public string Color { get; set; }
        public string Picture { get; set; }
        public List<string> List_invs { get; set; }
        public string Invs_selected { get; set; }
        public List<string> List_sers { get; set; }
        public string Sers_selected { get; set; }
        public long? BundleId { get; set; }
        public bool? DeviceRequired { get; set; }
        public string List_cuss { get; set; }
    }
    public class Jsonresult_view
    {
        public bool success { get; set; }
        public string messages { get; set; }
    }
}
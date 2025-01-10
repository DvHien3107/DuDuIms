using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel
{
    public class StoreCustomizeModel
    {

        /// <summary>
        /// list ds service chua thanh toan- order dang o trang thai submitted - completed not yet
        /// </summary>
        public class StoreServiceWaitingModel
        {
            public string Id { get; set; }
            public string StoreCode { get; set; }
            public string StoreName { get; set; }
            public string CustomerCode { get; set; }
            public Nullable<System.DateTime> EffectiveDate { get; set; }
            public Nullable<System.DateTime> RenewDate { get; set; }
            public string ProductCode { get; set; }
            public string Productname { get; set; }
            public string Product_Code_POSSystem { get; set; }
            public string Type { get; set; }
            public string Period { get; set; }
            public Nullable<bool> AutoRenew { get; set; }
            public string Status { get; set; }
            public string OrderCode { get; set; }
        }

        /// <summary>
        /// list ds invoice cua merchant trong merchant dashboard
        /// </summary>
        public class StoreInvoiceModel
        {
            public string  OrderCode { get; set; }
            public DateTime InvoiceDate { get; set; }
            public DateTime InvoiceDueDate { get; set; }
            public string Status { get; set; }
            public double Amount { get; set; }
            
        }


        //cau truc store push cho mango pos sau khi update store
        public class Store_DataModel
        {
            public string storeId { get; set; }//storeId la storeCode trong table C_Customer
            public string storeName { get; set; }
            public string ContactName { get; set; }
            public string Email { get; set; }
            public string CellPhone { get; set; }
            public string CreateBy { get; set; }
            public string CreateAt { get; set; }
            public string Status { get; set; }
            public string BusinessName { get; set; }
            public string BusinessPhone { get; set; }
            public string BusinessEmail { get; set; }
            public string BusinessAddress { get; set; }
            public string NewLicense { get; set; } = "0";
            public string Password { get; internal set; }
            public string RequirePassChange { get; set; } = "off";
            public string lastUpdate { get; set; }
            public string updateBy { get; set; }
            public List<ActiveProducts> activeProducts { get; set; }
            public List<Licenses> licenses { get; set; }
            
        }

        public class ActiveProducts
        {
            public string code { get; set; }
            public string name { get; set; }
        }

        public class Licenses
        {
            public string licenseCode { get; set; }
            public string licenseType { get; set; }
            public string subscription_warning_date { get; set; }
            public string subscription_warning_msg { get; set; }
            public string count_warning_value { get; set; }
            public string count_limit { get; set; }
            public string start_period { get; set; }
            public string end_period { get; set; }
            public string status { get; set; }
        }
    }

    public class Store_Services_Product_view
    {
        public Store_Services Store { get; set; }
        public License_Product Product { get; set; }
        public O_Orders Order { get; internal set; }
        public Order_Subcription Subscription { get; set; }
    }

    public class StoreReportView
    {
        public Store_Services Store { get; set; }
        public C_Customer Customer { get; set; }
        //public string WordDetermine { get; set; }
        //public string Owner { get; set; }
        //public string Phone { get; set; }
        //public string Bussiness { get; set; }
        public string OrderCode { get; set; }
        public bool? Status { get; set; }
        //public long CustomerId { get; set; }
        public DateTime? DueDate { get; set; } 
    }

    public class ServiceReportView
    {
        public Store_Services Store { get; set; }
        public Order_Subcription Subcription { get; set; }
        public C_Customer Customer { get; set; }
        public O_Orders Order { get; set; }
        public C_CustomerCard CustomerCard { get; set; }
        public C_PartnerCard PartnerCard { get; set; }
        public int? RemainingDate { get; set; }
        public string PartnerId { get; set; }
    }
    public class Subscription_History_view
    {
        public License_Product Product { get; set; }
        public List<Store_Services> Subscription { get; set; }
    }
}
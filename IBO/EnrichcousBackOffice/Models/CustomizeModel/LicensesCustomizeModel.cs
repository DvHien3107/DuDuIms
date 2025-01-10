using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel
{
    public class LicensesItemGroup
    {
        public License_Item_Group Group { get; set; }
        public List<License_Item> Items { get; set; }
        public List<Child_LicensesItemGroup> ChildGroups { get; set; }
    }
    public class Child_LicensesItemGroup
    {
        public License_Item_Group Group { get; set; }
        public List<License_Item> Items { get; set; }
    }
    public class LicensesGroupsView
    {
        public License_Item_Group Group { get; set; }
        public List<License_Item_Group> ChildGroups { get; set; }
    }

    public class LicenseProductView
    {
        public string NumberUpdated { get; set; }
        public bool UpdateStore { get; set; }
        public License_Product Product { get; set; }
        public List<License_Product_Item> Items { get; set; }
    }

    public class LicenseStoreActive
    {
        public string Id { get; set; }
        public string StoreName { get; set; }
        public string StoreCode { get; set; }
        public Nullable<int> Active{ get; set; }
        public string LicenseId { get; set; }
    }

    public class ProductItemPriceView
    {
        public long Prd_Item_Id { get; internal set; }
        public long ID { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
        public decimal? Price { get; set; }
        public string Period { get; set; }
        public bool? BuildIn { get; set; }
        public long License_Id { get; set; }
        public int? CountLimit { get; internal set; }
        public int? CountWarning { get; internal set; }
        public int? PeriodAmount { get; internal set; }
        public int? Value { get; internal set; }
        public string Type { get; internal set; }
    }
    public class DetailProductLicenseOrder
    {
        public bool? AutoRenew { get; set; }
        public DateTime? Effective_StartDate { get; set; }
        public DateTime? Expiry_Date { get; set; }
        public decimal? TotalAmount { get; set; }
        public IEnumerable<ProductItemPriceView> ProductItemPriceView { get; set; }
        public int? TrialMonths { get; set; }
        public string ProductType { get; set; }
        public int? Promotion_Apply_Months { get; set; }
        public decimal? Promotion_Price { get; set; }
        public decimal? RealPrice { get; set; }
        public string PriceType { get; set; }
        public int NumberOfPeriod { get; set; }
        public bool? Promotion_Apply_Status { get; set; }
        public string SubscriptionDuration { get; set; }
        public string SubscriptionId { get; set; }
        public string PeriodRecurring { get; set; }
        public int? SubscriptionQuantity { get; set; }
        public bool ApplyPaidDate { get; set; }
        public decimal? RecurringPrice { get; set; }

    }
}
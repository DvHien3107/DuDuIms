using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel
{
    public class Order_Products_view
    {
        public Order_Products order_model { get; set; }
        public List<device_info> device_Infos { get; set; }
        public class device_info
        {
            public string inv_number { get; set; }
            public string serial_number { get; set; }
        }
    }
    public class Order_Package_view
    {
        public I_Bundle Package { get; set; }
        public List<Order_Products_view> Products { get; set; }
    }
    public class Device_Service_ModelCustomize
    {
        public long? Key { get; set; }
        public string Type { get; set; }//Type: Device or Service
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string SubscriptionId { get; set; }
        public string SubscriptionName { get; set; }
        public string Picture { get; set; }
        public int Quantity { get; set; }
        public string Serial { get; set; }
        public decimal Price { get; set; }//gia cua product hoac gia cua dv mua them
        public decimal PartnerPrice { get; set; }//gia from Partner cua product hoac gia cua dv mua them
        public decimal MembershipPrice { get; set; }//gia from Partner cua product hoac gia cua dv mua them
        public decimal RealPrice { get; set; }
        public decimal PriceApply { get; set; }
        public int TrialMonths { get; set; }
        public decimal? Promotion_Price { get; set; }
        public string PriceType { get; set; }
        public int Promotion_Apply_Months { get; set; }
        public int NumberOfPeriod { get; set; }
        public bool Promotion_Apply_Status { get; set; }
        public int Promotion_Time_To_Available { get; set; }
        public bool IsMangoPOS { get; set; }
        public decimal? SetupFee { get; set; }
        public decimal? InteractionFee { get; set; }
        public decimal? MonthlyFee { get; set; }//so tien dv hang thang
        public DateTime? StartDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool ShortAge { get; set; }
        public string Description { get; set; }
        public string Feature { get; set; }
        public string ServicePlan { get; set; }
        public string ModelCode { get; set; }
        public string ModelName { get; set; }
        public long? BundleId { get; set; }
        public string BundleName { get; set; }
        public long? VendorId { get; set; }
        public string VendorName { get; set; }
        public decimal Amount { get; set; }
        public int Remaining_amount { get; set; }
        public List<Bundle_Model_view> list_Bundle_Device { get; set; }
        public string PeriodRecurring { get; set; }
        public string SubscriptionDuration { get; internal set; }
        public decimal Discount { get; internal set; }
        public decimal DiscountPercent { get; internal set; }
        public string DiscountType { get; internal set; }
        public bool? AutoRenew { get; set; }
        public bool? ApplyDiscountAsRecurring { get; set; }
        public int PreparingDays { get; set; }
        public int SubscriptionQuantity { get; set; }
        public bool ApplyPaidDate { get; set; }
        public decimal? RecurringPrice { get; set; }
    }
    public class Bundle_Model_view
    {
        public string ProductName { get; set; }
        public string ModelCode { get; set; }
        public string ModelName { get; set; }
        public string Color { get; set; }
        public int? Quantity { get; set; }
        public decimal? Price { get; set; }
    }
    public class OrderViewListModel
    {
        public O_Orders order { get; set; }
        public DateTime? updateAt { get; set; }
        public bool? isActivedLicense { get; set; }
        public string SalonEmail { get; set; }
        public string SalonPhone { get; set; }
        public string SalonName { get; set; }
        public string OwnerEmail { get; set;}
        public string OwnerPhone { get; set; }
        public string StoreCode { get; set; }
    }
}
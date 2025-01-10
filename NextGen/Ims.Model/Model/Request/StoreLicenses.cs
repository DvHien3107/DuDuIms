using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Model.Model.Request
{
    public class StoreLicenses
    {
        [Required(ErrorMessage = "storeId is Required")]
        public string storeId { get; set; }
        [Required(ErrorMessage = "storeName is Required")]
        public string storeName { get; set; }
        public string StoreEmail { get; set; }
        public string StorePhone { get; set; }

        [StringLength(500)]
        public string StoreAddress { get; set; } = "";

        [StringLength(500)]
        public string City { get; set; } = "";

        [StringLength(500)]

        public string State { get; set; } = "";

        public string ZipCode { get; set; } = "";

        public string ContactName { get; set; }
        public string lastUpdate { get; set; }
        public string CompanyPartner { get; set; }
        public string SubscriptionCode { get; set; }
        public string SubscriptionName { get; set; }
        public string UpdateBy { get; set; }

        [Required(ErrorMessage = "Store Email is required")]
        [StringLength(100, ErrorMessage = "Store Email max length 100")]

        public string Password { get; set; }
        public string RequirePassChange { get; set; }

        [Required(ErrorMessage = "Status is Required")]
        public string Status { get; set; }
        public DateTime? BusinessStartDate { get; set; }

        [Required(ErrorMessage = "TimeZone is Required")]
        public string TimeZone { get; set; }
        public string TimeZoneName { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // public List<ActiveProducts> activeProducts { get; set; }
        [Required(ErrorMessage = "Licenses is Required")]
        public List<Licenses> licenses { get; set; }
    }

    //public class ActiveProducts
    //{
    //    [Required(ErrorMessage = "code of ActiveProduct is required")]
    //    public string code { get; set; }
    //    public string name { get; set; }
    //}

    public class Licenses
    {
        [Required(ErrorMessage = "licenseCode is Required")]
        public string licenseCode { get; set; }
        public string licenseType { get; set; }
        [Required(ErrorMessage = "count_limit is Required")]
        public int count_limit { get; set; }
    }

    public class PairCode
    {
        public string Code { get; set; }
    }

    public class StoreLicensesJs
    {
        public StoreLicensesJs()
        {
            licenses = new List<LicensesJs>();
        }
        public string storeId { get; set; }
        public string storeName { get; set; }
        public string ContactName { get; set; }
        public string lastUpdate { get; set; }
        public string UpdateBy { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [StringLength(100, ErrorMessage = "Email max length 100")]
        [EmailAddress(ErrorMessage = "Email invalidate")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
        public string RequirePassChange { get; set; }
        public string CellPhone { get; set; }
        public string CreateBy { get; set; }
        public DateTime? CreateAt { get; set; }

        [Required(ErrorMessage = "Status is Required")]
        public string Status { get; set; }
        public string BusinessName { get; set; }
        public string BusinessPhone { get; set; }
        public string BusinessEmail { get; set; }
        public string BusinessAddress { get; set; }
        public DateTime? BusinessStartDate { get; set; }
        //public List<ActiveProducts> activeProducts { get; set; }
        public string NewLicense { get; set; }
        public List<LicensesJs> licenses { get; set; }

        public class LicensesJs
        {
            public string licenseCode { get; set; }
            public string licenseType { get; set; }
            public int count_limit { get; set; }
            public decimal? count_current { get; set; }
            public DateTime? start_period { get; set; }
            public DateTime? end_period { get; set; }
            public string status { get; set; }
            public string link { get; set; }
            public List<PairCodeJs> PairCodes { get; set; }

        }

        public class PairCodeJs
        {
            [Key]
            public string PairingCode { get; set; }
            public string PairingStatus { get; set; }
        }
    }
}

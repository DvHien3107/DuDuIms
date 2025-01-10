using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EnrichcousBackOffice.Utils.IEnums;
using Inner.Libs.Helpful;
using Newtonsoft.Json;

namespace EnrichcousBackOffice.ViewModel
{
    public class StoreProfile
    {
        public string StoreId { get; set; }
        public string StoreName { get; set; }
        public string ContactName { get; set; }
        public string LastUpdate { get; set; }
        public string UpdateBy { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string RequirePassChange { get; set; } = "off";
        public string CellPhone { get; set; }
        public string CreateBy { get; set; }
        public string CreateAt { get; set; }
        public string Status { get; set; }
        public string BusinessName { get; set; }
        public string BusinessPhone { get; set; }
        public string BusinessEmail { get; set; }
        public string BusinessAddress { get; set; }
        public string BusinessStartDate { get; set; }
        public string NewLicense { get; set; } = "0";
        public List<Licenses> Licenses { get; set; }
        public List<ActiveProducts> activeProducts { get; set; }
    }
    public class StoreProfileReq
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
        public string Password { get; set; }
        public string RequirePassChange { get; set; }
        public string Status { get; set; }
        public DateTime? BusinessStartDate { get; set; }
        public string TimeZone { get; set; }
        public string TimeZoneName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<Licenses> licenses { get; set; }
    }

    public class LicensesReq
    {
        public int Rollover { get; set; }
        public string LicenseCode { get; set; }
        public string LicenseType { get; set; }
        public string Count_limit { get; set; } = "0";
        public string Start_period { get; set; }
        public string End_period { get; set; }
    }

    public class ActiveProducts
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        //public string EffectiveDate { get; set; }
        //public string ExpiresDate { get; set; }
    }

    public class Licenses
    {
        public string LicenseCode { get; set; }
        public string LicenseType { get; set; }
        public string Subscription_warning_date { get; set; }
        public string Subscription_warning_msg { get; set; }
        public string Count_warning_value { get; set; }
        public string Count_limit { get; set; } = "0";
        public string Count_current { get; set; }
        public string Start_period { get; set; }
        public string End_period { get; set; }
        public string Status { get; set; }
        public string Link { get; set; }
        public List<Pairing> PairCodes { get; set; }
    }

    public class Pairing
    {
        public string PairingCode { get; set; }
        public string PairingStatus { get; set; }
    }

    public class ApiPOSResponse
    {
        public string Result { private get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public bool IsSuccess()
        {
            return StatusCode<int>() < 300;
        }
        public int StatusCode<T>()
        {
            return int.Parse(StatusCode());
        }
        public string StatusCode()
        {
            if ((Status == "success" || Result == "success"))
            {
                return "200";
            }
            return (Status ?? Result)?.Split('.')[0] ?? "400";
        }
    }

    public class ApiStoreProfileResponse : ApiPOSResponse
    {
        public List<StoreProfile> Data { get; set; }
    }

    public class StoreProfileV2 : StoreProfile
    {
        public string timeZone { get; set; } = TIMEZONE_NUMBER.Eastern.Text();
        public string timeZoneName { get; set; } = TIMEZONE_NUMBER.Eastern.ToString();
    }

    public class ApiStoreProfileResponseV2 : ApiPOSResponse
    {
        public List<StoreProfileV2> Data { get; set; }
    }
}
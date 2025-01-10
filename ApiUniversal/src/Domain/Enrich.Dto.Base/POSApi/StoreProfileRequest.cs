using Enrich.Common.Enums;
using Enrich.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Dto.Base.POSApi
{
    public class StoreProfileRequest
    {
        public string StoreId { get; set; }
        public string StoreName { get; set; }
        public string StorePhone { get; set; }
        public string StoreEmail { get; set; }
        public string StoreAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string ContactName { get; set; }
        public string LastUpdate { get; set; }
        public string UpdateBy { get; set; }
        public string Password { get; set; }
        public string RequirePassChange { get; set; } = "off";
        public string Status { get; set; }
        public string NewLicense { get; set; } = "0";
        public string SubscriptionName { get; set; }
        public string SubscriptionCode { get; set; }
        public string CompanyPartner { get; set; }
        public string TimeZone { get; set; } = EnumHelper.DisplayName(TIMEZONE.Eastern);
        public string TimeZoneName { get; set; } = TIMEZONE.Eastern.ToString();
        public List<LicensesRequest> Licenses { get; set; }
    }

    public class LicensesRequest
    {
        public int Rollover { get; set; }
        public string LicenseCode { get; set; }
        public string LicenseType { get; set; }
        public string Count_limit { get; set; } = "0";
        public string Start_period { get; set; }
        public string End_period { get; set; }
    }

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
        public string TimeZone { get; set; } = EnumHelper.DisplayName(TIMEZONE.Eastern);
        public string TimeZoneName { get; set; } = TIMEZONE.Eastern.ToString();
        public List<BaseService> Licenses { get; set; }
        public List<ActiveProducts> activeProducts { get; set; }
    }

    public class BaseService
    {
        public string LicenseCode { get; set; }
        public string LicenseType { get; set; }
        public string Subscription_warning_date { get; set; }
        public string Subscription_warning_msg { get; set; }
        public int Count_warning_value { get; set; }
        public int Count_limit { get; set; } = 0;
        public int Count_current { get; set; }
        public string Start_period { get; set; }
        public string End_period { get; set; }
        public string Status { get; set; }
        public string Link { get; set; }
        public List<Pairing> PairCodes { get; set; }
    }

    public class ActiveProducts
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
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
        public string Total { get; set; }
        public List<MangoFeature> DataResult { get; set; }
        public List<GiftcardData> Data { get; set; }

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

    public class MangoFeature
    {
        public string IdKey { get; set; }
        public string FeatureNames { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public bool RequestNum { get; set; }
        public int NumRequest { get; set; }
    }
    public class GiftcardData
    {
        public string ControlNumber { get; set; }
        public string QRCode { get; set; }
        public string Printed { get; set; }
    }
    public class FeatureBase
    {
        public string License_Item_Code { get; set; }
        public int? Value { get; set; }
        public int? Quantity { get; set; }
        public string SupscriptionType { get; set; }
        public string FeatureType { get; set; }
    }
}

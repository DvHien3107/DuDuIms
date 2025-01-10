using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Model.Model.Table.RCP
{
    public class Store
    {
        [Key]
        public int StoreID { get; set; }

        public string RoyaltyFee { get; set; }

        public string StoreName { get; set; }

        public string Logo { get; set; }

        public string CompanyName { get; set; }

        public string ContactPerson { get; set; }

        public string ContactAddress { get; set; }

        public string AddressLine { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string ZipCode { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string ContractID { get; set; }

        public string Comments { get; set; }

        public string DatabaseName { get; set; }

        public string DBUser { get; set; }

        public string DBPassword { get; set; }

        public string ServerID { get; set; }

        public string MerchantID { get; set; }

        public string MerchantKey { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? LastChange { get; set; }

        public bool StatusContract { get; set; }

        public string Website { get; set; }

        public int? TotalEmail { get; set; }

        public int? TotalMessenger { get; set; }

        public int? CurrentSMS { get; set; }

        public int? CurrentEmail { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public DateTime? CreateDate { get; set; }

        public int? NumPOSSystem { get; set; }

        public int? NumCheckInApp { get; set; }

        public bool? IsPlus { get; set; }

        public string SubscriptionName { get; set; }

        public string SubscriptionCode { get; set; }

        public string Coordinates { get; set; }

        public bool? NeedChange { get; set; }

        public DateTimeOffset? RefreshTokenExpiryTime { get; set; }

        public int? LoginFail { get; set; }

        public string RefreshToken { get; set; }

        public string TimeZone { get; set; }

        public string TimeZoneName { get; set; }

    }

}

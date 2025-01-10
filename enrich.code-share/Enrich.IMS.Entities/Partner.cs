namespace Enrich.IMS.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class Partner
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Address { get; set; }
        public string Hotline { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Logo { get; set; }
        public string ContactName { get; set; }
        public int? SalesSharePercent { get; set; }
        public string Description { get; set; }
        public int? Status { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime? CreateAt { get; set; }
        public string UpdateBy { get; set; }
        public System.DateTime? UpdateAt { get; set; }
        public string Url { get; set; }
        public string CheckinUrl { get; set; }
        public string ManageUrl { get; set; }
        public string LoginUrl { get; set; }
        public string PosApiUrl { get; set; }
        public string PriceType { get; set; }
        public long? MxMerchant_Id { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zipcode { get; set; }
        public string Country { get; set; }
        public string KeyLicense { get; set; }
    }
}

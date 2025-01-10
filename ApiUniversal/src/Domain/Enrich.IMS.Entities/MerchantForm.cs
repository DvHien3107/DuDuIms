namespace Enrich.IMS.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class MerchantForm
    {
        public long Id { get; set; }
        public string MerchantCode { get; set; }
        public string MerchantName { get; set; }
        public string TemplateName { get; set; }
        public string PDF_URL { get; set; }
        public string Status { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime? CreateAt { get; set; }
        public string UpdateBy { get; set; }
        public System.DateTime? UpdateAt { get; set; }
        public long? OrderId { get; set; }
        public long? TerminalId { get; set; }
        public bool? Signed { get; set; }
        public bool? Send { get; set; }
        public System.DateTime? SendAt { get; set; }
        public string AgreementId { get; set; }
        public string SendByAgent { get; set; }
        public bool? forNUVEI { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel.Report
{
    public class ExpiresReportModel
    {
        public string Id { get; set; }
        public string CustomerId { get; set; }
        public string PartnerCode { get; set; }
        public string StoreCode { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? RenewDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string ProductName { get; set; }
        public bool? AutoRenew { get; set; }
        public string OrderCode { get; set; }
        public string StoreName { get; set; }
        public bool? Status { get; set; }

        public string CustomerCode { get; set; }
        public DateTime? LastRenewAt { get; set; }
        public string LastUpdateBy { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel
{
    public class PODetail_vendor_view
    {
        public Vendor Vendor { get; set; }
        public PO_Detail detail { get; set; }
    }
    public class POhistory_model
    {
        public PO PO { get; set; }
        public PO_Detail Detail { get; set; }
    }
    public class PO_manager_view
    {
        public PO PO { get; set; }
        public List<PO_Detail_view> Detail { get; set; }
    }

    public class PO_Detail_view
    {
        public PO_Detail Detail { get; set; }
        public PO_Request Detail_request { get; set; }
        public List<PO_Detail_Checkin> Checkedin { get; set; }
    }
    public class PO_Detail_vendor
    {
        public PO_Detail Detail { get; set; }
        public long? VendorId { get; set; }
        public bool Purchased { get; set; }
    }

    public class PO_manager_detail_view
    {
        public string RequestCode { get; set; }
        public long? VendorId { get; set; }
        public string VendorName { get; set; }
        public decimal? Price { get; set; }
        public int? Qty { get; set; }
    }
    public class PurchaseOrder_view
    {
        public PO PO { get; set; }
        public List<PO_Detail_view> Detail { get; set; }
    }
}
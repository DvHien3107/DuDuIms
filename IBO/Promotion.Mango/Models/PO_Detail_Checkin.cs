//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Promotion.Mango.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class PO_Detail_Checkin
    {
        public long Id { get; set; }
        public Nullable<long> PO_Detail_id { get; set; }
        public string LocationId { get; set; }
        public Nullable<int> Qty { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedAt { get; set; }
        public string POCode { get; set; }
        public string InvNumbers { get; set; }
        public string LocationName { get; set; }
    }
}

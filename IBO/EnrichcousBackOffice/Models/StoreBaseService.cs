//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EnrichcousBackOffice.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class StoreBaseService
    {
        public int Id { get; set; }
        public string StoreCode { get; set; }
        public string KeyName { get; set; }
        public Nullable<int> RemainingValue { get; set; }
        public Nullable<int> MaximumValue { get; set; }
        public Nullable<System.DateTime> CreateAt { get; set; }
        public string CreateBy { get; set; }
        public string UpdateBy { get; set; }
        public Nullable<System.DateTime> UpdateAt { get; set; }
    }
}

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
    
    public partial class C_MerchantProcessing
    {
        public int Id { get; set; }
        public string TPN { get; set; }
        public string RegisterID { get; set; }
        public string Auth { get; set; }
        public string Note { get; set; }
        public Nullable<System.DateTime> UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public Nullable<long> MerchantSubscribeId { get; set; }
    }
}

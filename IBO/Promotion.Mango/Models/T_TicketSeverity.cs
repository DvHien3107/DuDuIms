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
    
    public partial class T_TicketSeverity
    {
        public long Id { get; set; }
        public string SeverityName { get; set; }
        public Nullable<int> SeverityLevel { get; set; }
        public string UpdateBy { get; set; }
        public Nullable<System.DateTime> UpdateAt { get; set; }
        public Nullable<bool> Active { get; set; }
        public string SpecialType { get; set; }
    }
}

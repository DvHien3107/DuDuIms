//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Enrich.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class License_Item
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public Nullable<long> GroupID { get; set; }
        public string GroupName { get; set; }
        public string Code { get; set; }
        public Nullable<bool> Enable { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public Nullable<bool> BuiltIn { get; set; }
    }
}

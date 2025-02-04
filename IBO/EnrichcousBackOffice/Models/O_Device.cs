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
    
    public partial class O_Device
    {
        public long DeviceId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public Nullable<int> Inventory { get; set; }
        public string Description { get; set; }
        public string TechnicalDescription { get; set; }
        public string Picture { get; set; }
        public string UpdateBy { get; set; }
        public Nullable<System.DateTime> UpdateAt { get; set; }
        public Nullable<bool> Active { get; set; }
        public string CreateBy { get; set; }
        public Nullable<System.DateTime> CreateAt { get; set; }
        public string AddonDevicesId { get; set; }
        public string AddonDevicesName { get; set; }
        public Nullable<bool> Junkyard { get; set; }
        public string JunkyardDescription { get; set; }
        public string Version { get; set; }
        public Nullable<System.DateTime> WarrantyExpiration { get; set; }
        public string InvNumber { get; set; }
        public string ModelCode { get; set; }
        public string SerialNumber { get; set; }
        public string LocationId { get; set; }
        public string LocationName { get; set; }
        public Nullable<long> VendorId { get; set; }
        public string VendorName { get; set; }
        public string ModelName { get; set; }
        public string Comment { get; set; }
    }
}

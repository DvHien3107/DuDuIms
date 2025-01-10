using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel
{
    public class ViewBundelDevices
    {
        public long Id { get; set; }
        public string BundleCode { get; set; }
        public string Name { get; set; }
        public DateTime? CreateAt { get; set; }
        public string Createby { get; set; }
        public string Info { get; set; }
        public List<O_Device> Listdevices { get; set; }
        public string OrderCode { get; internal set; }
    }
    public class Dashboard_Bundle
    {
        public string OrderCode { get; set; }
        public string CustomerName { get; set; }
        public int Id { get; set; }
        public string Status { get; set; }
        public string Agent { get; set; }
        public DateTime? UpdateAt { get;set; }
        public string UpdateBy { get; set; }
    }
    public class BundleDeviceView
    {
        public long DeviceId { get; set; }
        public string SerialNumber { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string Picture { get; set; }
    }
    public class Bundle_view
    {
        public I_Bundle Bundle { get; set; }
        public List<Bundle_Detail_view> Detail { get; set; }
    }

    public class Bundle_Detail_view
    {
        public I_Bundle_Device BundleDevice { get; set; }
        public O_Product_Model Model { get; set; }
    }
}
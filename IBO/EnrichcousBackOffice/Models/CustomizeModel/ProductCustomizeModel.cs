using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel
{
    public class DeviceSelectList
    {
        public long DeviceId { get; set; }
        public string SerialNumber { get; set; }
    }
    public class ProductView
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Picture { get; set; }
        public List<DeviceSelectList> Devices { get; set; }
        public string ProductLineCode { get; set; }
    }
    public class Feature
    {
        public string feature { get; set; }
        public int quantity { get; set; }
    }
    public class ProductFeatureView
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Picture { get; set; }
        public string ProductLineCode { get; set; }
        public List<I_Bundle_Device> Features { get; set; }
    }

    public class ProductInfoModel
    {
        public long Id { get; set; }
        public string ModelCode { get; set; }
        public string SerialNo { get; set; }
        public string InvNo { get; set; }
        public string ProductName { get; set; }


    }

}
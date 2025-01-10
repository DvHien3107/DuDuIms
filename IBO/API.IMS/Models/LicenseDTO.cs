using System.Collections.Generic;

namespace API.IMS.Models
{
    public class ResponseLicense
    {
        public string status { get; set; }
        public string message { get; set; }
        public List<License> data { get; set; }
    }
    public class ResponsePaymentOrder
    {
        public string status { get; set; }
        public string message { get; set; }
        public object addonResult { get; set; }
        public ResponseOrder data { get; set; }
    }
    public class ResponseOrder
    {
        public string OrderCode { get; set; }
        public decimal Total { get; set; }
        public string Url { get; set; }
    }

    public class License
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Type { get; set; }
        public string SubscriptionDuration { get; set; }
        public int SubscriptionEndWarningDays { get; set; }
        public int TrialDay { get; set; }
        public string Code { get; set; }
        public bool AllowDemo { get; set; }
        public bool AllowSlice { get; set; }
        public int PromoPrice { get; set; }
        public int PromoOfMonth { get; set; }
        public int Level { get; set; }
        public List<BaseService_Define> BasesServices { get; set; }
        public List<Feature> Features { get; set; }
    }

    /// <summary>
    /// Base service define
    /// </summary>
    public class BaseService_Define
    {
        public string licenseCode { get; set; }
        public string licenseType { get; set; }
        public string subscription_warning_date { get; set; }
        public string subscription_warning_msg { get; set; }
        public int count_warning_value { get; set; }
        public int count_limit { get; set; }
        public string status { get; set; }
    }
    /// <summary>
    /// Base service
    /// </summary>
    public class BaseService
    {
        public string serviceCode { get; set; }
        public string serviceName { get; set; }
        //public string subscription_warning_date { get; set; }
        //public string subscription_warning_msg { get; set; }
        //public int count_warning_value { get; set; }
        public int count_limit { get; set; }
        public string start_period { get; set; }
        public string end_period { get; set; }
        public string status { get; set; }
    }

    /// <summary>
    /// Feature
    /// </summary>
    public class Feature
    {
        public string IdKey { get; set; }
        public string FeatureNames { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public bool RequestNum { get; set; }
        public int NumRequest { get; set; }
    }
}
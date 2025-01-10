using System.Xml.Serialization;
using EnrichcousBackOffice.Utils.IEnums;
using Inner.Libs.Helpful;

namespace EnrichcousBackOffice.Utils.AppConfig
{
    [XmlRoot("root")]
    public class AppData
    {
        [XmlElement("store-url")]
        public StoreUrl StoreUrl { get; set; }
        public string StoreProfileUrl(string domain = "")
        {
            return (string.IsNullOrEmpty(domain) ? StoreUrl.V2.Domain : domain) + StoreUrl.V2.StoreProfile;
        }
        public string StoreChangeUrl(string domain = "")
        {
            return (string.IsNullOrEmpty(domain) ? StoreUrl.V2.Domain : domain) + StoreUrl.V2.StoreChange;
        }
        public string GetDefineFeatureUrl(string domain = "")
        {
            return (string.IsNullOrEmpty(domain) ? StoreUrl.V2.Domain : domain) + StoreUrl.V2.GetDefineFeature;
        }
        public string AddDefineFeatureUrl(string domain = "")
        {
            return (string.IsNullOrEmpty(domain) ? StoreUrl.V2.Domain : domain) + StoreUrl.V2.AddDefineFeature;
        }
        public string RemoveDefineFeatureUrl(string domain = "")
        {
            return (string.IsNullOrEmpty(domain) ? StoreUrl.V2.Domain : domain) + StoreUrl.V2.RemoveDefineFeature;
        }
        public string GetDefineFeatureByStoreUrl(string domain = "")
        {
            return (string.IsNullOrEmpty(domain) ? StoreUrl.V2.Domain : domain) + StoreUrl.V2.GetDefineFeatureByStore;
        }
        public string HardResetUrl(string domain = "")
        {
            return (string.IsNullOrEmpty(domain) ? StoreUrl.V2.Domain : domain) + StoreUrl.V2.HardReset;
        }
        public string SoftResetUrl(string domain = "")
        {
            return (string.IsNullOrEmpty(domain) ? StoreUrl.V2.Domain : domain) + StoreUrl.V2.SoftReset;
        }
        public string DelPairingCodeUrl(string domain = "")
        {
            return (string.IsNullOrEmpty(domain) ? StoreUrl.V2.Domain : domain) + StoreUrl.V2.DelPairingCode;
        }
        public string OrderGiftCardUrl(string domain = "")
        {
            return (string.IsNullOrEmpty(domain) ? StoreUrl.V2.Domain : domain) + StoreUrl.V2.OrderGiftCard;
        }
        public string CheckinLink(string domain = "")
        {
            return string.IsNullOrEmpty(domain) ? StoreUrl.V2.CheckinLink : domain;
        }
        public string SaloncenterLink(string domain = "")
        {
            return string.IsNullOrEmpty(domain) ? StoreUrl.V2.SaloncenterLink : domain;
        }
        public string ReportSMSusedlist(string domain = "")
        {
            return (string.IsNullOrEmpty(domain) ? StoreUrl.V2.Domain : domain) + StoreUrl.V2.ReportSMSusedlist;
        }
        public string ReportSMSusedstore(string domain = "")
        {
            return (string.IsNullOrEmpty(domain) ? StoreUrl.V2.Domain : domain) + StoreUrl.V2.ReportSMSusedstore;
        }
        [XmlElement("mango-pos")]
        public MangoPOS MangoPOS { get; set; }
        [XmlElement("mango-demo-trial")]
        public MangoDemoTrial MangoDemoTrial { get; set; }
        [XmlElement("boss-manage")]
        public BossManage BossManage { get; set; }
    }
    public class StoreUrl
    {
        [XmlElement("VER2")]
        public StoreUrlVersion V2 { get; set; }
    }
    public class StoreUrlVersion
    {
        [XmlAttribute("domain")]
        public string Domain { get; set; }
        [XmlAttribute("store-change")]
        public string StoreChange { get; set; }
        [XmlAttribute("store-profile")]
        public string StoreProfile { get; set; }
        [XmlAttribute("define-feature")]
        public string GetDefineFeature { get; set; }
        [XmlAttribute("add-feature")]
        public string AddDefineFeature { get; set; }
        [XmlAttribute("remove-feature")]
        public string RemoveDefineFeature { get; set; }
        [XmlAttribute("get-feature")]
        public string GetDefineFeatureByStore { get; set; }
        [XmlAttribute("saloncenter_link")]
        public string SaloncenterLink { get; set; }
        [XmlAttribute("saloncenter_slice_link")]
        public string SaloncenterSliceLink { get; set; }
        [XmlAttribute("checkin_link")]
        public string CheckinLink { get; set; }
        [XmlAttribute("hard-reset")]
        public string HardReset { get; set; }
        [XmlAttribute("soft-reset")]
        public string SoftReset { get; set; }
        [XmlAttribute("DelPairingCode")]
        public string DelPairingCode { get; set; }
        [XmlAttribute("OrderGiftCard")]
        public string OrderGiftCard { get; set; }
        [XmlAttribute("reportSMSusedlist")]
        public string ReportSMSusedlist { get; set; }
        [XmlAttribute("reportSMSusedstore")]
        public string ReportSMSusedstore { get; set; }
    }
    public class BossManage
    {
        [XmlElement("url")]
        public BossUrl Url { get; set; }
    }
    public class BossUrl
    {
        [XmlAttribute("add_edit")]
        public string AddEdit { get; set; }
        [XmlAttribute("add_store")]
        public string AddStore { get; set; }
        [XmlAttribute("get_store")]
        public string GetStore { get; set; }
        [XmlAttribute("del_store")]
        public string DelStore { get; set; }
        [XmlAttribute("leave_boss")]
        public string LeaveBoss { get; set; }
        [XmlAttribute("change_pass")]
        public string ChangePass { get; set; }
    }
    public class MangoPOS
    {
        [XmlElement("timezone-url")]
        public string TimeZoneUrl { get; set; }
    }
    public class MangoDemoTrial
    {
        [XmlElement("trial-duration-days")]
        public string TrialDuration { get; set; }
    }
}
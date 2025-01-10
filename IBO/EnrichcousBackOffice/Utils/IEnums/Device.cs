using Inner.Libs.Helpful;

namespace EnrichcousBackOffice.Utils.IEnums
{ 
    public enum OrderCarriers
    {
        [EnumAttr("UPS Ground")] UPS_Ground,
        [EnumAttr("FedEx Ground")] FedEx_Ground,
        [EnumAttr("USPS")] USPS,
        [EnumAttr("Delivery")] Delivery,
    }
}
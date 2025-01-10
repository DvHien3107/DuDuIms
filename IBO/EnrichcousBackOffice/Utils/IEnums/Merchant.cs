using Inner.Libs.Helpful;
using System.ComponentModel.DataAnnotations;

namespace EnrichcousBackOffice.Utils.IEnums
{

    /// <summary>
    /// IMS Version
    /// </summary>
    public enum IMSVersion
    {
        [EnumAttr("v1", "POS Version 1")] POS_VER1 = 0,
        [EnumAttr("v2", "POS Version 2")] POS_VER2 = 1,
    }

    public enum MerchantType
    {

        [EnumAttr("STORE_OF_MERCHANT", "Store of merchant")] STORE_OF_MERCHANT,
        [EnumAttr("STORE_IN_HOUSE", "Store in house")] STORE_IN_HOUSE
    }
    public enum MerchantForm_filter
    {

        [EnumAttr("ALL")] ALL = 0,
        [EnumAttr("Lead")] Lead = 1,
        [EnumAttr("Merchant")] Merchant = 2,
    }

    public enum MerchantOptionEnum
    {
        [EnumAttr("Merchant.Processor", "Processor")] Processor,
        [EnumAttr("Merchant.Source", "Source")] Source,
        [EnumAttr("Merchant.TerminalType", "Terminal Type")] TerminalType,
        [EnumAttr("Merchant.DeviceName", "DeviceName")] DeviceName,
        [EnumAttr("Merchant.POSStructure", "POSStructure")] POSStructure,
        [EnumAttr("Merchant.DeviceSetupStructure", "DeviceSetupStructure")] DeviceSetupStructure,
    }
    public enum MerchantStatusSearch
    {
        [EnumAttr("All")] All = 0, // store service (active = 1) or ( (terminal status is null or empty) and MID is not null) 
        [EnumAttr("Live merchant (terminal or software)")] LiveMerchant = 1, // store service (active = 1) or ( (terminal status is null or empty) and MID is not null) 
        [EnumAttr("Software only (live)")] OnlyLicense = 2, // store service (active = 1) and renew date < GETUTCDATE
        [EnumAttr("Terminal only (live)")] OnlyTerminal = 3, // store service (active = 1) and renew date < GETUTCDATE
        [EnumAttr("Software and terminal (live)")] LicenseTerminal = 4, // store service (active = 1) and renew date < GETUTCDATE
        [EnumAttr("Expired license and active terminal")] ExpiredLicenseActiveTerminal = 5, // store service (active = 1) and renew date < GETUTCDATE
        [EnumAttr("Expired license and inactive terminal")] ExpiredLicenseInactiveTerminal = 6, // store service (active = 1) and renew date < GETUTCDATE
        [EnumAttr("Inactive license and inactive terminal")] InactiveLicenseInactiveTerminal = 7, // store service (active = 1) and renew date < GETUTCDATE
        [EnumAttr("Canceled account")] Cancel = 8, // search from cancle date
    }
}
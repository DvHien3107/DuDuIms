using Inner.Libs.Helpful;
using System.ComponentModel.DataAnnotations;

namespace EnrichcousBackOffice.Utils.IEnums
{


    public enum LicenseDefinded
    {
        [EnumAttr("MangoSlice")] MANGO_SLICE
        
    }


    /// <summary>
    /// Enum type of license services
    /// </summary>
    public enum LicenseType
    {
        [EnumAttr("license")] LICENSE,
        [EnumAttr("addon")] ADD_ON,
        [EnumAttr("other")] VirtualHardware_Other,
        [EnumAttr("giftcard")] GiftCard,
    }

    /// <summary>
    /// Store services stage
    /// </summary>
    public enum LicenseStatus
    {
        [EnumAttr(-3, "Reset")] RESET,
        [EnumAttr(-2, "Removed")] REMOVED,
        [EnumAttr(-1, "Waiting")] WAITING,
        [EnumAttr(0, "DeActive")] DE_ACTIVE,
        [EnumAttr(1, "Active")] ACTIVE,
    }

    public enum TIMEZONE_NUMBER
    {
        [EnumAttr(-5, "-05:00")] Eastern,
        [EnumAttr(-6, "-06:00")] Central,
        [EnumAttr(-7, "-07:00")] Mountain,
        [EnumAttr(-8, "-08:00")] Pacific,
        [EnumAttr(+7, "+07:00")] VietNam,
    }
    public enum TIMEZONE_NUMBER_BY_ID
    {
        [EnumAttr("Eastern Standard Time", "-05:00")] Eastern,
        [EnumAttr("Central Standard Time", "-06:00")] Central,
        [EnumAttr("Mountain Standard Time", "-07:00")] Mountain,
        [EnumAttr("Pacific Standard Time", "-08:00")] Pacific,
        [EnumAttr("SE Asia Standard Time", "+07:00")] VietNam,
    }

    public enum Store_Apply_Status
    {
        [EnumAttr("Trial")] Trial,
        [EnumAttr("Promotional")] Promotional,
        [EnumAttr("Real")] Real,

    }
    // for universal
    public enum ServiceType
    {
        [Display(Name = "Trial account")] Merchant_Trial,
        [Display(Name = "Merchant with License")] Merchant_License,
        [Display(Name = "Merchant with MID")] Merchant_MID,
        [Display(Name = "Merchant with License and MID")] Merchant_License_MID
    }
    public enum LicenseStatusUniversal
    {
        [Display(Name = "InActive")] InActive = 0, // Store_Services.active != 1 
        [Display(Name = "Expired")] Expired = 50,// Store_Services.active = 1 
        [Display(Name = "Active")] Active = 100,// Store_Services.active = 1 
        [Display(Name = "ActiveInfuture")] FutureActive = 200 // Store_Services.active = 1 
    }
    public enum TerminalStatus
    {
        [Display(Name = "InActive")] InActive = 0, // Store_Services.active != 1 
        [Display(Name = "Active")] Active = 1,// Store_Services.active = 1 
    }
}
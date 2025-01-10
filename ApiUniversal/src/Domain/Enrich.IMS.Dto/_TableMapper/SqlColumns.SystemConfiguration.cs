using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto
{
    public sealed partial class SqlColumns
    {
        public sealed class SystemConfiguration
        {
            public const string AdobeRefreshToken = "Adobe_refresh_token";
            public const string DocuSignAccessToken = "DocuSign_access_token";
            public const string DocuSignRefreshToken = "DocuSign_refresh_token";
            public const string LastScanDate = "Last_scan_date";
            public const string MxMerchantAccessSecret = "MxMerchant_AccessSecret";
            public const string MxMerchantAccessToken = "MxMerchant_AccessToken";
            public const string UPSShipAttentionName = "UPS_ShipAttentionName";
            public const string UPSShipperAccountNumber = "UPS_ShipperAccountNumber";
            public const string UPSShipperAddress = "UPS_ShipperAddress";
            public const string UPSShipperCellPhone = "UPS_ShipperCellPhone";
            public const string MxMerchantRequestSecret = "MxMerchant_RequestSecret";
            public const string MxMerchantRequestToken = "MxMerchant_RequestToken";
            public const string UPSAccessLicenseNumber = "UPS_AccessLicenseNumber";
            public const string UPSAccountName = "UPS_AccountName";
            public const string UPSAccountNumber = "UPS_AccountNumber";
            public const string UPSAccountPassword = "UPS_AccountPassword";
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel.Merchant
{
    public class MerchantImportData
    {
        public string StoreCode { get; set; }
        public string SalePerson { get; set; }
        public string AccountManager { get; set; }
        public DateTime? GoliveDate { get; set; }
        public DateTime? CancelDate { get; set; }
        public string MID { get; set; }
        public string ProcessingPartner { get; set; }
        public string POSStructure { get; set; }
        public string DeviceName { get; set; }
        public string DeviceNote { get; set; }
        public string DeviceNameStructure { get; set; }
        public string ManagerPhone { get; set; }
    }
}
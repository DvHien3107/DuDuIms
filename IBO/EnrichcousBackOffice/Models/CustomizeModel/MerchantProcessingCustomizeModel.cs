
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel
{
    public partial class MerchantProcessingCustomizeModel
    {
        public long Id { get; set; }
        public string CustomerCode { get; set; }
        public string MerchantID { get; set; }
        public string ProcessorName { get; set; }
        public string Status { get; set; }
        public string[] CardTypeAccept { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public IEnumerable<C_MerchantProcessing> ListMerchantProcess { get; set; }
    }
}
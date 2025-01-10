using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel
{
    public class MerchantFormView
    {
        public List<O_MerchantForm> List_pdf;
        public long Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string WordDetermine { get; set; }
    }

    public class AccessTokenDocuSignModel
    {
        public string access_token { get; set; }
        public DateTime expires_time { get; set; }
    }

}
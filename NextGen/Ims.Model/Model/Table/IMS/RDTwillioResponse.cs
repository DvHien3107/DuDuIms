using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Model.Model.Table.IMS
{
    public class RDTwillioResponse
    {
        [Key]
        public decimal ID {  get; set; }

        public string StoreCode { get; set; }

        public int? MaxRVCNo { get; set; }

        public int? CountRVCNo { get; set; }

        public string DataFrom { get; set; }

        public string Sid { get; set; }

        public string AuthToken { get; set; }

        public string VerificationStatus { get; set; }

        public string messaging_service_sid { get; set; }

        public string smsid { get; set; }

        public string tw_body { get; set; }

        public int? num_segments { get; set; }

        public string status { get; set; }

        public string error_message { get; set; }

        public DateTime? date_created { get; set; }

        public string direction { get; set; }

        public string fromNumber { get; set; }

        public string toNumber { get; set; }

        public string uri { get; set; }

        public string jsonResult { get; set; }

    }

}

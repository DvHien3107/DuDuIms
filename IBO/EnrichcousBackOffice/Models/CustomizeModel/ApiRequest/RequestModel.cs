using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel.ApiRequest
{
    public class RequestModel
    {
        public string Method { get; set; }
        public string Url { get; set; }
        public string JsonRequest { get; set; }
    }
}
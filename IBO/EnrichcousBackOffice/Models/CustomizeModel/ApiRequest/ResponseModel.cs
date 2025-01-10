using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel.ApiRequest
{
    public class ResponseModel
    {
        public ResponseModel()
        {
            Success = false;
        }
        public int StatusCode { get; set; }
        public string Url { get; set; }
        public string Method { get; set; }
        public bool? Success { get; set; }
        public string JsonResponse { get; set; }
    }
}
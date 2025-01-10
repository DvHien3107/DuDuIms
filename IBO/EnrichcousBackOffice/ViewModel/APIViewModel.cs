using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.ViewModel
{
    public class ResponseModel
    {
        public string RequestId { get; set; }
        public int Result { get; set; }
        public string Message { get; set; }
    }

    public class ResponseDataModel
    {
        public string RequestId { get; set; }
        public int Result { get; set; }
        public string Message { get; set; }
        public Object Data { get; set; }
    }

}
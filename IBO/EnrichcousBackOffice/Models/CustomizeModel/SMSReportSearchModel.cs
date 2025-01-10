using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel
{
    public class SMSReportSearchModel
    {
        //
        // Summary:
        //     Filter by messages by body
        public string Body { get; set; }
        //
        // Summary:
        //     Filter by messages sent to this number
        public string ToPhone { get; set; }
        //
        // Summary:
        //     Filter by from number
        public string FromPhone { get; set; }
        //
        // Summary:
        //     Filter by date sent
        public DateTime? DateSentBefore { get; set; }
        //
        // Summary:
        //   Filter by date sent
        // public DateTime? DateSent { get; set; }
        //
        // Summary:
        //     Filter by date sent
        public DateTime? DateSentAfter { get; set; }
        //
        // Summary:
        //     Filter by status
        public bool? Status { get; set; }

        //
        // Summary:
        //     Filter by Number Segments
        public int? NumSegments { get; set; }

    }

    public class ResponeApiSMSUsedReport { 
        public double status { get; set; }
        public string message { get; set; }
        public DataSMSUsedReport data { get; set; }
        
    }
    public class DataSMSUsedReport
    {
        public int total { get; set; }
        public List<DetailSMSUsedReport> list { get; set; }
    }
    public class DetailSMSUsedReport
    {
        public double id { get; set; }
        public string storeId { get; set; }
        public string storeName { get; set; }
        public string keySend { get; set; }
        public string sendFrom { get; set; }
        public string sendTo { get; set; }
        public bool? isEmail { get; set; }
        public string sendBy { get; set; }
        public DateTime? sendDate { get; set; }
        public bool? isSuccess { get; set; }
        public string sendData { get; set; }
        public string errorReturn { get; set; }
        public int? smsSegment { get; set; }
        public int? rvcNo { get; set; }
        public int? totalSent { get; set; }
    }
}
using System;

namespace Enrich.DataTransfer
{
    public class SMSHistoryRemaining
    {
        
        public int Id { get; set; }

        public string StoreCode { get; set; }

        public string Description { get; set; }

        public int ObjectId { get; set; }

        public int RemainingSMS { get; set; }

        public int UsedSegment { get; set; }

        public int TotalSegment { get; set; }

        public double Price { get; set; }

        public int SentMessage { get; set; }

        public int Type { get; set; }

        public string Caller { get; set; }

        public DateTime? CreatedDate { get; set; }
    }
}

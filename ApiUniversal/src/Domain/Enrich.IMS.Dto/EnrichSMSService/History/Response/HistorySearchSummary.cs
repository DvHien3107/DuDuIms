using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto.EnrichSMSService
{
    public class HistorySearchSummary
    {
        /// <summary>
        /// total price
        /// </summary>
        public decimal Price { get; set; }


        /// <summary>
        /// total message
        /// </summary>
        public int SMS { get; set; }


        /// <summary>
        ///total used segment
        /// </summary>
        public int UsedSegment { get; set; }


        /// <summary>
        ///total sms
        /// </summary>
        public int Segment { get; set; }

        /// <summary>
        ///total sms
        /// </summary>
        public int SimpleSMS { get; set; }

        /// <summary>
        /// total campaign
        /// </summary>
        public int Campaign { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto.Subscription
{
    public class TerminalStatusDto
    {
        public string MID { get; set; }
        /// <summary>
        /// status. 1 is active
        /// </summary>
        public int Status { get; set; }


        public int TerminalType { get; set; }
    }
}

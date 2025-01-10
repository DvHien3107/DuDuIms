using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Entities
{
    public class WebGridConfig
    {
        public int Id { get; set; }

        public int UserId { get; set; }    

        /// <summary>
        /// grid type, exp: merchant type, sale lead type
        /// </summary>
        public short Type { get; set; }

        public string ConfigAsJson { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto.WebGridConfig
{
    public class WebGridConfigDto
    {
        public int Id { get; set; }

        public int UserId { get; set; }    

        public short Type { get; set; }

        public string ConfigAsJson { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnrichcousBackOffice.Models.Respon
{
    public class rsData
    {
        public rsData()
        {
            Status = 500;
            Message = "Empty Content";
            Extended_data = new Dictionary<string, object>();
        }

        public Dictionary<string, object> Extended_data { get; set; }
        public int Status { get; set; }
        public string Message { get; set; }
        public object Obj_data { get; set; }
    }
}

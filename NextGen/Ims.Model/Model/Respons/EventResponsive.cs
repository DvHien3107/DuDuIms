using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Model.Model.Respons
{
    public class EventResponsive
    {
        public int status { get; set; }
        public string message { get; set; }
        public object data { get; set; }
        public dynamic dynamicData { get; set; }
    }
}

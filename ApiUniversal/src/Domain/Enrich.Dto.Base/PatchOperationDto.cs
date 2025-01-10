using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Dto
{
    public class PatchOperationDto
    {
        public string Field;

        public object Value;

        public string FullField { get; set; }
    }
}

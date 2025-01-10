using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Dto.Base
{
    public class KeyValueDto<TValue>
    {
        public string Key { get; set; }

        public TValue Value { get; set; }
    }
}

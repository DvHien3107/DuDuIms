using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Dto.Base.Responses
{
    public class CreateResponse<T>
    {
        public T Data { get; set; }
        public IEnumerable<string> Validations { get; set; }
    }
}

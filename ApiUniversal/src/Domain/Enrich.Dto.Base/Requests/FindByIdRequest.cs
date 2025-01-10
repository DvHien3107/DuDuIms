using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Dto.Requests
{
    public class FindByIdRequest
    {
        public object Id { get; set; }
        public string Fields { get; set; }
    }
}

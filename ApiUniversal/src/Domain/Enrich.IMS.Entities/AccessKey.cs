using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Entities
{
    public class AccessKey
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }

        public bool IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }

    }
}
